import sys, pathlib, struct, hashlib, math, json
sys.path.insert(0,str(pathlib.Path(__file__).parent/'analysis-deps'))
import pefile
from unicorn import Uc, UC_ARCH_X86, UC_MODE_64
from unicorn.x86_const import *
import argparse
parser=argparse.ArgumentParser()
parser.add_argument('--exe',required=True)
parser.add_argument('--output',default='pitch-hold-test-results.json')
args=parser.parse_args()
data=pathlib.Path(args.exe).read_bytes()
assert hashlib.sha256(data).hexdigest().upper()=='B627639CE4A3F8C940FE90AFA1D36ED59DB3FF9CED24E935890EB7C99B04D5BF'
pe=pefile.PE(data=data); image=pe.get_memory_mapped_image(); base=0x140000000
site=0x7cd8b; old=bytes.fromhex('66440f6ec2'); new=bytes.fromhex('eb0e909090')
assert pe.get_data(site,5)==old
assert pe.get_data(0x7cd82,9)==bytes.fromhex('84c0741583f9017531')
u=Uc(UC_ARCH_X86,UC_MODE_64);u.mem_map(base,(len(image)+4095)&~4095);u.mem_write(base,image)
u.mem_map(0x20000000,0x10000); bp=0x20002000;di=0x20004000;stack=0x20008000
def f(v): return struct.pack('<f',v)
def putx(reg,v):u.reg_write(reg,int.from_bytes(f(v),'little'))
def getx(reg):return struct.unpack('<f',(u.reg_read(reg)&0xffffffff).to_bytes(4,'little'))[0]
def run(patched,mode,current,inp,dt):
 u.mem_write(base+site,new if patched else old)
 u.ctl_remove_cache(base+site,base+site+16)
 u.reg_write(UC_X86_REG_RBP,bp);u.reg_write(UC_X86_REG_RDI,di);u.reg_write(UC_X86_REG_RSP,stack)
 u.reg_write(UC_X86_REG_RAX,mode);u.reg_write(UC_X86_REG_RDX,int.from_bytes(f(inp),'little'))
 for reg,val in [(UC_X86_REG_XMM14,current),(UC_X86_REG_XMM8,current),(UC_X86_REG_XMM12,0)]:putx(reg,val)
 u.mem_write(bp+0x1688,f(dt));u.mem_write(bp+0xc60,f(0));u.mem_write(stack+0x40,f(0));u.mem_write(di+0x4d0,f(1));u.mem_write(di+0x4d4,f(1))
 u.emu_start(base+0x7cd40,base+0x7cdbc,count=100)
 target=getx(UC_X86_REG_XMM8)
 # Execute original target clamp and pitch smoothing with existing -80/+80 mod.
 u.mem_write(base+0x7cf62,bytes.fromhex('d6b9b500'))
 u.mem_write(0x20005000,struct.pack('<fff',0,-1,0));u.reg_write(UC_X86_REG_RAX,0x20005000)
 putx(UC_X86_REG_XMM14,current);putx(UC_X86_REG_XMM12,0);putx(UC_X86_REG_XMM13,-0.0);putx(UC_X86_REG_XMM15,0);putx(UC_X86_REG_XMM10,dt)
 u.emu_start(base+0x7cf48,base+0x7d051,count=100)
 return target,current+getx(UC_X86_REG_XMM6)
checks=0
for pitch in [-79,-60,-20,0,20,60,79]:
 for hz in [30,60,144]:
  p=math.radians(pitch)
  target,out=run(True,1,p,0,1/hz)
  assert abs(target-p)<1e-6 and abs(out-p)<1e-6,(pitch,hz,target,out)
  checks+=1
  for inp in [-1,1]:
   target,out=run(True,1,p,inp,1/hz)
   assert math.isfinite(out) and abs(out)<=math.radians(80)+1e-5
   assert (out-p)*inp>0,(pitch,hz,inp,out)
   checks+=1
for mode in [0,2,3]:
 for p in [-0.7,0.7]:
  for inp in [-1,0,1]:
   assert run(False,mode,p,inp,1/60)==run(True,mode,p,inp,1/60)
   checks+=1
assert abs(run(False,1,0.7,0,1/60)[0])<1e-8
for p in [0.65,-0.45,0.2,-0.8]*25:
 assert abs(run(True,1,p,0,1/60)[1]-p)<1e-6
 checks+=1
out={'checks_passed':checks,'sha256':hashlib.sha256(data).hexdigest(),'rva':hex(site),'original':old.hex(),'replacement':new.hex(),'scope':'Actual x64 target/clamp/smoothing instructions emulated; no game-client or live deployment test.'}
pathlib.Path(args.output).write_text(json.dumps(out,indent=2))
print(json.dumps(out,indent=2))

