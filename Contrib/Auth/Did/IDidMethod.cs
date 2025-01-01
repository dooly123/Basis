using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DidUrlFragment = Basis.Contrib.Auth.Did.Newtypes.DidUrlFragment;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace Basis.Contrib.Auth.Did
{
	/// The functionality that all DID methods implement
	interface IDidMethod
	{
		/// Resolves to a map of DID Url fragments to a Json Web Key. This method
		/// resolves a DID to its DID Document, and inspects the `verificationMethods`
		/// field to extract a dictionary of public keys.
		///
		/// Even though json is not what all DID methods will use to represent keys,
		/// we standardize the api to return JsonWebKey because it documents its
		/// own key algorithms and is a reasonably portable format.
		public Task<ReadOnlyDictionary<DidUrlFragment, JsonWebKey>> ResolvePubkeys();
	}
}
