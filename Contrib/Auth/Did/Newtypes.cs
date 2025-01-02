// This file contains various wrapper types, to more safely differentiate them
// and help code document itself.

namespace Basis.Contrib.Auth.DecentralizedIds.Newtypes
{
	/// A DID. DIDs do *not* contain any fragment portion. See
	/// https://www.w3.org/TR/did-core/#did-syntax
	public record Did(string V);

	/// A full DID Url, which is a did along with an optional path query and
	/// fragment. See
	/// https://www.w3.org/TR/did-core/#did-url-syntax
	public record DidUrl(string V);

	/// A DID Url Fragment. Does not include the `#` part.
	public record DidUrlFragment(string V);

	/// A JSON Web Signature, serialized in "compact serialization" form.
	public record JwsCompact(string V);
}
