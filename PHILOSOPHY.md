# Philosophy of Basis

> [!NOTE]
It is entirely possible that things will change. Treat this document as malleable and our *best estimate* of the future. It is not a guarantee.

## What is Basis?

Basis is an open source MIT licensed framework for building immersive VR games.
It lowers the barrier of entry for game developers and is the *basis* upon which
those games can build.

To be specific, Basis is a **set of libraries** designed to help you bootstrap
your VR projects. Basis is also a **reference implementation** that
demonstrates how these components can work together.

It is built on top of Unity and C#, and the reference implementation in
particular makes use of the Universal Rendering Pipeline (URP) for rendering,
and targets IL2CPP.

## Who is this for?

This is for game developers and creators. Most but not all people interested in
basis fit into one or multiple of these categories:

- You want to build a singleplayer VR game with high quality full-body tracking
  and avatars
- You want to build a multiplayer social VR game or platform
- You want to host events in VR and bypass limitations of other platforms
- You want to contribute to FOSS (Free and Open Source Software)
- You want to tinker, experiment, and create

In short, Basis is whatever you want it to be, minus the hard foundational work
that limits your creativity or agency. Use basis as a starting point to build
what you want.

A rising tide lifts all ships. By providing a foundation to support VR
projects, we can make the medium more accessible and help foster creativity.

## What are the project goals?

Basis seeks to build best-in-class implementations for the following core
areas:

- **Presence**: The feeling, based on your passive senses, that you exist in a
  space.  
- **Spatialization**: The feeling that phenomena (especially your actions)
  exist in, and have an active effect on, objects in physical space.  
- **Embodiment**: The association between a representation of a body and the
  real body itself, created by continuous feedback between action and reaction
  within a physical body.  
- **Social**: *apis and frameworks* for multiplayer features, such as
  VOIP, authentication to instances, instance moderation, asset retrieval, etc.


> [!NOTE]
Social features will be optional modules - singleplayer game devs that want to
use basis won't need them.

Basis aims to offer these building blocks to creators, and the reference
implementation will be a batteries-included place for us to [dogfood][dogfood]
the framework.

## Non-goals

The following are non-goals, and are out of scope for both the framework and the
reference implementation:

- **Content Delivery Networks** - instead of us building backend services for a
  CDN, you are responsible for hosting your own content on your own CDN. We will
  provide batteries included integrations for popular CDNs, but building a CDN
  ourselves or moderating content on it is out of scope.
- **Paid features** - the reference implementation is not a profit vehicle and we
  have no plans to monetize it with paid features. All code in the reference
  implementation will be free and open source. We *may* provide or recommend
  paid hosting services to make it possible for non-technical users to spin up
  their own servers.
- **Non-realtime social features** - we do not plan to build any services other
  than what is strictly necessary for a good multiplayer experience in a realtime
  instance. In practice this means we would build into the framework pluggable
  apis for handling usernames, allow/denylists, blocking and muting users, and
  other similarly essential social functionality. But more extensive features
  that require us to maintain infrastructure, moderate users and content, or
  build large social networks etc are out of scope. 
- **Centralized services** - building and maintaining centralized services requires
  centralized financial and technical support. If done poorly, these features
  would burden the project with maintenance issues. If done well, they would
  likely be closed off and monetized, defeating the purpose of Basis as an open
  toolset to empower creators. Basis seeks to have minimal if any dependencies
  on external services - none will be required in the framework itself, and if
  any are integrated into the reference implementation, it will be done in a
  minimally invasive and either federated, P2P, or self-hostable fashion.

[dogfood]: https://en.wikipedia.org/wiki/Eating_your_own_dog_food
