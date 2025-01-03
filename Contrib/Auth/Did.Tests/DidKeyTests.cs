using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Basis.Contrib.Auth.DecentralizedIds.Newtypes;
using Xunit;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace Basis.Contrib.Auth.DecentralizedIds
{
	public class DidKeyTests
	{
		[Fact]
		public async Task DidKeyTestVectors()
		{
			// See https://w3c-ccg.github.io/did-method-key/#ed25519-x25519
			var examples = new List<(string, JsonWebKey)>
			{
				(
					"did:key:z6MkiTBz1ymuepAQ4HEHYSF1H8quG5GLVVQR3djdX3mDooWp",
					new JsonWebKey()
					{
						Kty = "OKP",
						Crv = "Ed25519",
						X = "O2onvM62pC1io6jQKm8Nc2UyFXcd4kOmOsBIoYtZ2ik",
					}
				),
			};

			var resolver = new DidKeyResolver();
			foreach (var (inputDid, expectedJwk) in examples)
			{
				var expectedFragment = new DidUrlFragment(
					inputDid.Split(
						DidKeyResolver.PREFIX,
						count: 2,
						options: StringSplitOptions.RemoveEmptyEntries
					)[0]
				);
				var document = await resolver.ResolveDocument(new Did(inputDid));
				Debug.Assert(document.Pubkeys.Count == 1);
				var resolvedJwk = document.Pubkeys[expectedFragment];
				Debug.Assert(
					JsonSerializer.Serialize(resolvedJwk)
						== JsonSerializer.Serialize(expectedJwk),
					"resolved JWK did not match expected JWK"
				);
			}
		}
	}
}
