using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DidUrlFragment = Basis.Contrib.Auth.Did.Newtypes.DidUrlFragment;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace Basis.Contrib.Auth.Did
{
	/// Implements resolution of a did:key to the various information stored in it
	class DidKeyResolver : IDidMethod
	{
		public Task<ReadOnlyDictionary<DidUrlFragment, JsonWebKey>> ResolvePubkeys()
		{
			throw new System.NotImplementedException("todo");
		}
	}
}
