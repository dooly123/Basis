using System;
using Basis.Contrib.Auth.DecentralizedIds.Newtypes;
using CryptoRng = System.Security.Cryptography.RandomNumberGenerator;

namespace Basis.Contrib.Auth.DecentralizedIds
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
		/// Number of bytes in a nonce. This is currently 256 bits.
		// TODO(@thebutlah): Decide if its too performance intensive to use 256
		// bits, and if 128 bit would be sufficient.
		const ushort NONCE_LEN = 256 / sizeof(byte);

		// We store the rng to make deterministic testing and seeding possible.
		readonly CryptoRng Rng;

		public DidAuthentication(Config cfg)
		{
			Rng = cfg.Rng;
		}

		public Challenge MakeChallenge(Did identity)
		{
			var nonce = new byte[NONCE_LEN];
			Rng.GetBytes(nonce);
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

	/// Challenges are a randomized nonce. The nonce will be the payload
	/// that is signed by the user's private key. Generating a random nonce
	/// for every authentication attempt ensures that an attacker cannot
	/// perform a [replay attack](https://en.wikipedia.org/wiki/Replay_attack).
	///
	/// Challenges also track the identity of the party that the challenge was
	/// sent to, so that later the signature's public key can be compared to
	/// the identity's public key.
	public record Challenge(Did Identity, byte[] Nonce);

	public record Response(
		/// A JSON Web Signature, in "compact serialization" form. The payload
		/// of the JWS are the bytes of the corresponding challenge's nonce.
		JwsCompact Jws,
		/// The particular key in the user's did document. If the empty string,
		/// it is implied that there is only one key in the document and that
		/// this single key should be what is used as the pub key.
		///
		/// Examples:
		/// * `""`
		/// * `"key-0"`
		/// * `"z6MkhaXgBZDvotDkL5257faiztiGiC2QtKLGpbnnEGta2doK"`
		DidUrlFragment DidUrlFragment
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
