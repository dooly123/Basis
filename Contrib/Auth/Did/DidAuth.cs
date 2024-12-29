/// Cryptographically secure random number generator
using CryptoRng = System.Security.Cryptography.RandomNumberGenerator;

namespace Basis.Contrib.Auth.Did
{
	public record Config
	{
		public CryptoRng Rng { get; init; } = CryptoRng.Create();
	}

	// TODO: Create and implement an `IChallengeResponseAuth` interface. This interface should live in basis core.
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
		///
		/// If successfull in verifying the response, returns null, otherwise
		/// returns the VerifyResponseErr
		// TODO(@thebutlah): Is this way of doing errors OK with C# devs?
		public VerifyResponseErr? VerifyResponse(Response response, Challenge challenge)
		{
			// TODO(@thebutlah): Implement this
			return null;
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

	// TODO(@thebutlah): Implement this
	public record Response();

	/// Possible errror variants for the VerifyResponse method.
	public enum VerifyResponseErr
	{
		MismatchedNonce,
		MismatchedPubKey,
	}
}
