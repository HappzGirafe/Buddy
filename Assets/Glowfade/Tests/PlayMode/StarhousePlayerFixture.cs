using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

namespace Glowfade.Tests
{
    /// Shared setup for tests that play the real Starhouse scene and drive its player with a virtual keyboard.
    public abstract class StarhousePlayerFixture : InputTestFixture
    {
        protected Keyboard keyboard;
        protected FirstPersonController player;
        protected CharacterController body;
        protected Transform eyes;

        protected IEnumerator LoadStarhouse()
        {
            yield return SceneManager.LoadSceneAsync("Starhouse", LoadSceneMode.Single);
            player = Object.FindAnyObjectByType<FirstPersonController>();
            Assert.That(player, Is.Not.Null, "the Starhouse scene needs a player");
            body = player.GetComponent<CharacterController>();
            eyes = player.cameraPivot;
            Assert.That(eyes, Is.Not.Null, "the player needs a camera");
            keyboard = InputSystem.AddDevice<Keyboard>();
            keyboard.MakeCurrent();
            yield return null;
        }

        /// Puts the player's feet at a world position (just above it), facing the given compass direction,
        /// and lets them settle. Their stance is kept.
        protected IEnumerator PutPlayer(Vector3 feet, float yaw)
        {
            body.enabled = false;
            player.transform.SetPositionAndRotation(feet + Vector3.up * 0.05f, Quaternion.Euler(0f, yaw, 0f));
            body.enabled = true;
            yield return new WaitForSeconds(0.3f);
        }

        protected IEnumerator Tap(KeyControl key)
        {
            Press(key);
            yield return null;
            Release(key);
            yield return null;
        }

        protected IEnumerator Hold(KeyControl key, float seconds)
        {
            Press(key);
            yield return new WaitForSeconds(seconds);
            Release(key);
            yield return null;
        }

        /// How far the player has moved across the floor since `from`.
        protected float Flat(Vector3 from)
        {
            Vector3 d = player.transform.position - from;
            d.y = 0f;
            return d.magnitude;
        }

        /// Finds a named object inside one of the Starhouse's locations, e.g. ("Wing_LampPostMeadow", "HollowLog").
        protected static Transform FindInLevel(string location, string name)
        {
            var level = GameObject.Find("Starhouse");
            Assert.That(level, Is.Not.Null, "the scene needs its Starhouse root");
            var root = level.transform.Find(location);
            Assert.That(root, Is.Not.Null, $"no location called {location}");
            var found = FindDeep(root, name);
            Assert.That(found, Is.Not.Null, $"no {name} in {location}");
            return found;
        }

        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform child in t)
            {
                var found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        /// The compass direction (yaw) that faces along a world direction.
        protected static float YawOf(Vector3 direction) => Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, Vector3.up)).eulerAngles.y;
    }
}
