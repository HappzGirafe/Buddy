using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Glowfade.Tests
{
    /// Plays the real Starhouse and tries its hiding spots, seats and steps with the player's stances:
    /// crawling into hiding places lying down, sitting on furniture, stepping up from a corridor.
    /// The places are found by name, so a renamed or moved prop shows up here.
    public class LevelStanceTests : StarhousePlayerFixture
    {
        [UnityTest]
        public IEnumerator Meadow_HollowLog_LieDownAndCrawlInside()
        {
            yield return LoadStarhouse();
            var log = FindInLevel("Wing_LampPostMeadow", "HollowLog");
            // lie down 0.8 m outside its west end, facing along the log
            yield return PutPlayer(log.TransformPoint(new Vector3(-2f, 0f, 0f)), YawOf(log.right));
            yield return Tap(keyboard.zKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "Z lies down");

            yield return Hold(keyboard.wKey, 2f / player.crawlSpeed);
            Vector3 inside = log.InverseTransformPoint(player.transform.position);
            Assert.That(inside.x, Is.InRange(-0.7f, 0.7f), "crawled into the middle of the log");
            Assert.That(Mathf.Abs(inside.z), Is.LessThan(0.15f), "straight down the middle");
        }

        [UnityTest]
        public IEnumerator Meadow_StageManagerDesk_LieDownAndCrawlUnder()
        {
            yield return LoadStarhouse();
            var desk = FindInLevel("Wing_LampPostMeadow", "StageManagerDesk");
            // lie down in front of its open knee space (the desk's local +Z side), beside the stool, and crawl in
            yield return PutPlayer(desk.TransformPoint(new Vector3(-0.25f, 0f, 1.45f)), YawOf(-desk.forward));
            yield return Tap(keyboard.zKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "Z lies down");

            yield return Hold(keyboard.wKey, 1.35f / player.crawlSpeed);
            Vector3 under = desk.InverseTransformPoint(player.transform.position);
            Assert.That(under.z, Is.LessThan(0.3f), "crawled under the desk top");
            Assert.That(Mathf.Abs(under.x), Is.LessThan(0.5f), "between its side panels");
        }

        [UnityTest]
        public IEnumerator EchoLab_DissectionBench_LieDownAndCrawlUnderFromTheBack()
        {
            yield return LoadStarhouse();
            var bench = FindInLevel("Wing_EchoLab", "DissectionBench");
            // lie down in the gap behind its open back (local -Z), facing it, and crawl in
            yield return PutPlayer(bench.TransformPoint(new Vector3(0f, 0f, -1f)), YawOf(bench.forward));
            yield return Tap(keyboard.zKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "Z lies down");

            yield return Hold(keyboard.wKey, 0.9f / player.crawlSpeed);
            Vector3 under = bench.InverseTransformPoint(player.transform.position);
            Assert.That(under.z, Is.InRange(-0.4f, 0.35f), "crawled under the bench top, up to its front panel");
        }

        [UnityTest]
        public IEnumerator Kitchen_DiningChairs_CanBeSatOn([Values("Chair_North", "Chair_East")] string chairName)
        {
            yield return LoadStarhouse();
            var chair = FindInLevel("Wing_MidnightKitchen", chairName);
            // stand in front of the seat, facing the chair
            yield return PutPlayer(chair.TransformPoint(new Vector3(0f, 0f, 1.3f)), YawOf(-chair.forward));
            yield return Tap(keyboard.xKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Sitting), "X sits");
            Assert.That(player.IsOnSeat, Is.True, "on the chair, not the floor");
            Assert.That(player.transform.position.y - chair.position.y, Is.EqualTo(0.6f).Within(0.05f), "on the seat");
        }

        [UnityTest]
        public IEnumerator Booth_ProjectionistsCot_CanBeSatOn()
        {
            yield return LoadStarhouse();
            var cot = FindInLevel("FrontOfHouse", "Cot");
            // stand at its open east side, facing it
            yield return PutPlayer(cot.TransformPoint(new Vector3(1.05f, 0f, 0.3f)), YawOf(-cot.right));
            yield return Tap(keyboard.xKey);
            Assert.That(player.IsOnSeat, Is.True, "on the cot, not the floor");
            Assert.That(player.transform.position.y - cot.position.y, Is.EqualTo(0.5f).Within(0.05f), "on the mattress");
        }

        [UnityTest]
        public IEnumerator Tank_WalkInFromTheCorridor_OntoTheDryLanding()
        {
            yield return LoadStarhouse();
            var tank = FindInLevel("Wing_DeepDarkTank", "Wing_DeepDarkTank");
            // start in the corridor 1 m outside the door, facing in
            yield return PutPlayer(tank.TransformPoint(new Vector3(0f, 0f, -1f)), YawOf(tank.forward));
            yield return Hold(keyboard.wKey, 1.2f);
            Vector3 p = tank.InverseTransformPoint(player.transform.position);
            Assert.That(p.z, Is.InRange(0.5f, 3f), "walked in through the door");
            Assert.That(p.y, Is.EqualTo(0.38f).Within(0.1f), "stepped up onto the dry landing above the flood");
        }
    }
}
