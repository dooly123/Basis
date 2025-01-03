using System.Collections.ObjectModel;
using DidUrlFragment = Basis.Contrib.Auth.DecentralizedIds.Newtypes.DidUrlFragment;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace Basis.Contrib.Auth.DecentralizedIds
{
	/// Contains the info that we care about in the DID Document.
	/// A DID Document is what a DID is resolved into. See
	/// https://www.w3.org/TR/did-core/#did-resolution
	public record DidDocument(ReadOnlyDictionary<DidUrlFragment, JsonWebKey> Pubkeys);
}
