# Phone 3D map: HTTP/LAN secure-context diagnosis

**Prepared:** October 20, 2026. **Scope:** source-only diagnostic and safe fallback. The companion remains paused for broader development. **Nothing here changes the live Enshrouded server.**

## What is demonstrated

- The authenticated ShroudForge phone companion is served at a local **`http://<private-LAN-address>:<port>/<private-key>/`** endpoint by `PhoneView.cs`. This is a local network HTTP page, not HTTPS.
- The GameVault 3D map URL itself is **HTTPS**. Its iframe on an HTTP parent does **not** have a browser secure context under the Secure Contexts specification. Secure Web Crypto APIs, including `crypto.subtle.digest`, require a secure context.
- The owner previously demonstrated that the external map renders **inside a separate HTTPS diagnostic page** on iPhone Safari with Compatibility mode. The owner later supplied evidence of a `crypto.subtle.digest` failure on the actual phone integration. These are consistent with insecure parent ancestry, but without device inspection we must not treat the exact failing GameVault script as independently proven.

Authoritative references: [MDN Secure contexts](https://developer.mozilla.org/en-US/docs/Web/Security/Defenses/Secure_Contexts), [W3C Secure Contexts — ancestral risk](https://www.w3.org/TR/secure-contexts/), [MDN SubtleCrypto.digest](https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/digest).

## Staged safe behavior

The phone's Map tab checks `window.isSecureContext` **in the parent page**. If it is explicitly false, the page displays an explanatory warning and an **Open the 3D map directly over HTTPS** link. This external link uses `target="_blank"`, `rel="noopener noreferrer"`, and `referrerpolicy="no-referrer"` so the private ShroudForge URL is not supplied as an HTTP referrer or opener.

The existing manual **Load interactive map** option is preserved (some browser/provider combinations might work), as are the Compatibility/Protected toggle, map troubleshooting, and the no-referrer iframe. No third-party map is loaded until the user taps Load. Existing authenticated server controls and token handling are unchanged. Standalone HTML and compiled C# fallback remain synchronized.

**Not a full fix:** A warning/link cannot grant Web Crypto to an HTTPS iframe inside an HTTP ancestor. The map provider cannot be changed by ShroudForge. Do not implement an insecure cryptography polyfill, bypass third-party origin policy, proxy third-party scripts/tiles, expose the server via public HTTP, or claim embedded terrain works.

## Isolated acceptance checks (when phone access is authorized)

1. **Private LAN HTTP phone page:** open Map without loading. The insecure-context warning and direct HTTPS link should appear; server controls should continue working. No map provider request before clicking Load.
2. **Open full map:** tap the direct external link. It should open the official GameVault HTTPS URL in a separate top-level page, never ShroudForge's private credential URL; verify 3D terrain and touch controls manually.
3. **Embedded attempt:** tap Load, then optionally Protected/Compatibility. If the iframe remains blank or reports `crypto.subtle.digest`, treat the embedded result as **not fixed**. The direct HTTPS page is the supported fallback for the current HTTP architecture.
4. **Desktop / CI:** compile both Windows launchers and check phone HTML synchronization, authentication isolation, and expected warning/link markup. These checks cannot certify physical-device rendering.

## Future true embedded-map option

A genuine embedded fix requires delivering the **entire ShroudForge parent phone page** in a browser-trusted HTTPS context and testing the third-party provider again. It entails TLS certificate trust on the actual iPhone, private-host binding, token handling, access restrictions and safe local network topology. This must be designed and reviewed separately, **without exposing management controls on the public internet**. Do not simply flip `http` to `https` in the `TcpListener` URL: it does not supply a valid TLS server or trusted certificate.

**Status:** source diagnostics implemented, device confirmation and HTTPS architecture outstanding.
