import sys,struct,math,json,hashlib,argparse
from pathlib import Path
parser=argparse.ArgumentParser();parser.add_argument('--exe',required=True);parser.add_argument('--deps',default='');parser.add_argument('--output',required=True);parser.add_argument('--fixture-image');args=parser.parse_args()
if args.deps:sys.path.insert(0,str(Path(args.deps).resolve()))
import pefile
from unicorn import Uc,UC_ARCH_X86,UC_MODE_64,UC_HOOK_MEM_WRITE
from unicorn.x86_const import *
host=Path(args.exe)
b=host.read_bytes();assert hashlib.sha256(b).hexdigest().upper()=='B627639CE4A3F8C940FE90AFA1D36ED59DB3FF9CED24E935890EB7C99B04D5BF'
pe=pefile.PE(data=b);base=pe.OPTIONAL_HEADER.ImageBase;image=pe.get_memory_mapped_image();u=Uc(UC_ARCH_X86,UC_MODE_64);u.mem_map(base,(len(image)+4095)&~4095);u.mem_write(base,image);u.mem_map(0x20000000,0x10000)
f=lambda v:struct.pack('<f',v)
def setx(r,v):u.reg_write(r,int.from_bytes(f(v),'little'))
def getx(r):return struct.unpack('<f',(u.reg_read(r)&0xffffffff).to_bytes(4,'little'))[0]
checks=0
for dt in [0,1/144,1/60,1/30,0.25,1]:
 for cost in [0,0.1,1,5,20,100]:
  for mod in [0,0.5,1,2]:
   for previous in [0,0.25,7,100]:
    values=[]
    for patched in [False,True]:
     u.mem_write(base+0xd818f,bytes.fromhex('0f57f690' if patched else 'f30f59f7'))
     u.ctl_remove_cache(base+0xd818f,base+0xd8193)
     # Actual game instructions: float(dt)*cost*depletion modifier, then add to shared accumulator.
     setx(UC_X86_REG_XMM6,dt);setx(UC_X86_REG_XMM8,cost);setx(UC_X86_REG_XMM7,mod)
     u.emu_start(base+0xd818a,base+0xd8193,count=10)
     contribution=getx(UC_X86_REG_XMM6)
     u.reg_write(UC_X86_REG_RBP,0x20002000);u.mem_write(0x20002000-0x78,struct.pack('<Q',0x20004000));u.mem_write(0x20004000,f(previous))
     u.emu_start(base+0xd81ba,base+0xd81c6,count=10)
     result=struct.unpack('<f',u.mem_read(0x20004000,4))[0]
     expected=previous if patched else previous+dt*cost*mod
     assert math.isclose(result,expected,rel_tol=2e-6,abs_tol=1e-6),(patched,dt,cost,mod,previous,result,expected)
     if patched:assert contribution==0.0
     values.append(result);checks+=1
# Preserve abnormal/nonfinite calculated input: zero glider contribution must still be exactly zero.
for val in [float('inf'),float('-inf'),float('nan')]:
 setx(UC_X86_REG_XMM6,val);u.emu_start(base+0xd818f,base+0xd8193,count=3);assert getx(UC_X86_REG_XMM6)==0;checks+=1
assert struct.unpack('<Q',pe.get_data(0x13699c0,8))[0]==base+0xd7f40
assert pe.get_data(0x136cb90,len(b'stamina_depletion_glider\0'))==b'stamina_depletion_glider\0'
# Game image, common finalizer and sprint systems are never modified on disk.
assert host.read_bytes()==b
if args.fixture_image:
 sparse=bytearray(0x1400000)
 for rva,size in [(0xd7f40,0x312),(0x13699c0,8),(0x136cb90,24)]:sparse[rva:rva+size]=pe.get_data(rva,size)
 Path(args.fixture_image).write_bytes(sparse)
result={'instruction_checks_passed':checks,'glider_system_registration_verified':True,'prior_other_drain_preserved':True,'disk_game_unchanged':True,'scope':'Actual x64 arithmetic/accumulation instructions emulated; gameplay and vanilla-client prediction unverified.'}
Path(args.output).write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
