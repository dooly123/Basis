/// Cryptographically secure random number generator
using System;
using CryptoRng = System.Security.Cryptography.RandomNumberGenerator;

namespace Basis.Contrib.Auth.Did
{
	/// Configuration for [`DidAuthentication`].
	public record Config
	{
		public CryptoRng Rng { get; init; } = CryptoRng.Create();
	}

	// TODO(@thebutlah): Create and implement an `IChallengeResponseAuth`
	// interface. This interface should live in basis core.
	public class DidAuthentication
	{
		// We store the rng to make deterministic testing and seeding possible.
		readonly CryptoRng rng;

		public DidAuthentication(Config cfg)
		{
			this.rng = cfg.Rng;
		}

		public Challenge MakeChallenge(string identity)
		{
			var nonce = new byte[256 / sizeof(byte)]; // 256-bit nonce
			this.rng.GetBytes(nonce);
			return new Challenge(Identity: identity, Nonce: nonce);
		}

		/// Compares the response against the original challenge.
		///
		/// Ensures that:
		/// * The response signature matches the public keys of the challenge
		///   identity.
		/// * The response signature payload matches the nonce in the challenge
		///
		/// It is the caller's responsibility to keep track of which challenges
		/// should be held for which responses.
		public VerifyResponseResult VerifyResponse(
			Response response,
			Challenge challenge
		)
		{
			throw new NotImplementedException("todo");
		}
	}

	/// Challenges are a 256 bit randomized nonce. The nonce will be the payload
	/// that is signed by the user's private key. Generating a random nonce
	/// for every authentication attempt ensures that an attacker cannot
	/// perform a [replay attack](https://en.wikipedia.org/wiki/Replay_attack).
	///
	/// Challenges also track the identity of the party that the challenge was
	/// sent to, so that later the signature's public key can be compared to
	/// the identity's public key.
	public record Challenge(string Identity, byte[] Nonce);

	public record Response(
		/// A JSON Web Signature, in "compact serialization" form. The payload
		/// of the JWS are the bytes of the corresponding challenge's nonce.
		string Jws,
		/// The particular key in the user's did document. If the empty string,
		/// it is implied that there is only one key in the document and that
		/// this single key should be what is used as the pub key.
		///
		/// Examples:
		/// * `""`
		/// * `"#key-0"`
		/// * `"#z6MkhaXgBZDvotDkL5257faiztiGiC2QtKLGpbnnEGta2doK"`
		string DidUrlFragment
	);

	/// Possible return values VerifyResponse method.
	public enum VerifyResponseResult
	{
		/// The verification was successful
		Success,

		/// The fragment in the response didn't exist in the DID Document resolved
		/// from the challenge's identity.
		NoSuchFragment,

		/// The JWS Payload did not match the challenge nonce
		MismatchedNonce,

		/// The JWS verification failed due to an invalid signature
		InvalidSig,
	}
}
