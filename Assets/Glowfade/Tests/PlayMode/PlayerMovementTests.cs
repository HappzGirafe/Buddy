using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Glowfade.Tests
{
    /// Plays the real Starhouse scene and drives its player with a virtual keyboard:
    /// walking from the start, running, crouching, jumping, lying down and sitting.
    /// Most checks happen in a test yard built well below the Starhouse, so the geometry is known.
    /// Headless Unity runs thousands of frames a second, so these tests also catch movement that breaks at high frame rates.
    public class PlayerMovementTests : StarhousePlayerFixture
    {
        // close to the origin so tiny per-frame steps stay precise, and far below every floor of the Starhouse
        static readonly Vector3 Yard = new Vector3(0f, -60f, 0f);

        // lanes: open floor down the middle, a bench, a low shelf to crawl under, a table to crouch under, a staircase
        // a 0.5 m seat: the controller steps up 0.4 m (plus up to its 0.08 m skin), so a lower bench could be walked onto
        const float BenchLane = 8f, BenchTop = 0.5f, BenchFront = 1.0f, BenchDepth = 0.5f;
        const float CrawlLane = -8f, ShelfBottom = 0.92f, ShelfStart = 1.0f, ShelfDepth = 1.2f;
        const float TableLane = -15f, TableBottom = 1.25f, TableStart = 1.0f, TableDepth = 1.2f;
        const float StairLane = 15f, StairStart = 1.0f, StairRise = 0.3f, StairRun = 0.3f;
        const int StairSteps = 6;
        // a low desk (like the hub's cue desk) far down the middle lane: you can lie under it, its top is at seat height
        const float DeskLane = 0f, DeskBottom = 0.88f, DeskTop = 0.94f, DeskStart = 10f, DeskDepth = 1.2f;

        // ---------------------------------------------------------------- tests

        [UnityTest]
        public IEnumerator Start_StandsOnTheFloor_AndWalksForward()
        {
            yield return LoadStarhouse();
            yield return new WaitForSeconds(0.5f);
            Assert.That(body.isGrounded, Is.True, "the player should be standing on the floor at the start");

            Vector3 start = player.transform.position;
            yield return Hold(keyboard.wKey, 1f);
            Assert.That(Flat(start), Is.GreaterThan(1.5f), "W should walk forward from the start point");
            Assert.That(Mathf.Abs(player.transform.position.y - start.y), Is.LessThan(0.3f), "walking from the start shouldn't fall or climb");
        }

        [UnityTest]
        public IEnumerator Walk_Run_AndCrouch_EachHaveTheirOwnSpeed()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(0f, -15f);

            Vector3 from = player.transform.position;
            yield return Hold(keyboard.wKey, 1f);
            float walk = Flat(from);

            from = player.transform.position;
            Press(keyboard.leftShiftKey);
            yield return Hold(keyboard.wKey, 1f);
            Release(keyboard.leftShiftKey);
            yield return null;
            float run = Flat(from);

            yield return Tap(keyboard.cKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Crouching), "C crouches");
            Assert.That(body.height, Is.EqualTo(player.crouchHeight).Within(0.01f));
            from = player.transform.position;
            yield return Hold(keyboard.wKey, 1f);
            float crouch = Flat(from);

            Assert.That(walk, Is.InRange(player.walkSpeed * 0.8f, player.walkSpeed * 1.2f), "walking speed");
            Assert.That(run, Is.InRange(player.sprintSpeed * 0.8f, player.sprintSpeed * 1.2f), "running speed");
            Assert.That(crouch, Is.InRange(player.crouchSpeed * 0.7f, player.crouchSpeed * 1.3f), "crouching speed");

            yield return Tap(keyboard.cKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Standing), "C again stands up");
            Assert.That(body.height, Is.EqualTo(player.standHeight).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator Space_Jumps_AndLands()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(0f, -5f);

            float floor = player.transform.position.y, highest = floor;
            Press(keyboard.spaceKey);
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                yield return null;
                highest = Mathf.Max(highest, player.transform.position.y);
            }
            Release(keyboard.spaceKey);
            yield return new WaitForSeconds(0.5f);

            Assert.That(highest - floor, Is.GreaterThan(0.5f), "Space should jump");
            Assert.That(body.isGrounded, Is.True, "and land again");
        }

        [UnityTest]
        public IEnumerator Z_LiesDown_CrawlsUnderALowShelf_AndCanOnlyStandUpOutside()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(CrawlLane, 0f);

            yield return Tap(keyboard.zKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "Z lies down");
            Assert.That(body.height, Is.LessThan(0.75f));
            yield return new WaitForSeconds(0.5f);
            Assert.That(eyes.localPosition.y, Is.LessThan(0.45f), "lying down puts your eyes near the floor");

            // crawl until you're under the middle of the shelf
            float underMiddle = ShelfStart + ShelfDepth * 0.5f;
            yield return Hold(keyboard.wKey, underMiddle / player.crawlSpeed);
            Assert.That(player.transform.position.z - Yard.z, Is.InRange(ShelfStart + 0.2f, ShelfStart + ShelfDepth - 0.2f), "should have crawled under the shelf");

            yield return Tap(keyboard.zKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "no room to stand up under the shelf");
            yield return Tap(keyboard.cKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "no room to crouch under the shelf either");
            yield return Tap(keyboard.spaceKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "nor to get up with Space");

            // just past the far edge the shelf still covers the back of a standing body, so you can't stand up all the way
            // (a crouching body's rounded top does clear the edge, so Z gets you as far as crouching)
            yield return PutPlayerAt(CrawlLane, ShelfStart + ShelfDepth + 0.33f);
            yield return Tap(keyboard.zKey);
            Assert.That(player.CurrentStance, Is.Not.EqualTo(Stance.Standing), "the shelf's edge is still over a standing body");
            Assert.That(body.height, Is.LessThan(player.standHeight - 0.1f));

            // move clear and get up
            yield return Hold(keyboard.wKey, 0.8f);
            yield return Tap(keyboard.spaceKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Standing), "Space gets you up once there's room");
            Assert.That(body.height, Is.EqualTo(player.standHeight).Within(0.01f));

            // Z and Y swap places on German keyboards, so Y lies down too
            yield return Tap(keyboard.yKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "Y lies down too");
        }

        [UnityTest]
        public IEnumerator C_CrouchWalksUnderATable_AndCanOnlyStandUpOutside()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(TableLane, 0f);

            yield return Tap(keyboard.cKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Crouching), "C crouches");

            float underMiddle = TableStart + TableDepth * 0.5f;
            yield return Hold(keyboard.wKey, underMiddle / player.crouchSpeed);
            Assert.That(player.transform.position.z - Yard.z, Is.InRange(TableStart + 0.2f, TableStart + TableDepth - 0.2f), "should have crouch-walked under the table");

            yield return Tap(keyboard.cKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Crouching), "no room to stand up under the table");

            yield return PutPlayerAt(TableLane, TableStart + TableDepth + 0.33f);
            yield return Tap(keyboard.cKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Crouching), "the table's edge is still over you");

            yield return Hold(keyboard.wKey, 0.6f / player.crouchSpeed);
            yield return Tap(keyboard.cKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Standing), "C again stands up once there's room");
        }

        [UnityTest]
        public IEnumerator Stairs_CanBeWalkedUp()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(StairLane, 0f);

            float floor = player.transform.position.y;
            yield return Hold(keyboard.wKey, (StairStart + StairSteps * StairRun + 1f) / player.walkSpeed);
            Assert.That(player.transform.position.y - floor, Is.EqualTo(StairSteps * StairRise).Within(0.15f), "walked up to the top step");
        }

        [UnityTest]
        public IEnumerator X_SitsOnTheBenchInFront_AndGetsUpWhereYouStood()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(BenchLane, BenchFront - 0.5f);
            Vector3 stood = player.transform.position;

            yield return Tap(keyboard.xKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Sitting), "X sits");
            Assert.That(player.IsOnSeat, Is.True, "on the bench, not the floor");
            Assert.That(player.transform.position.y - Yard.y, Is.EqualTo(BenchTop).Within(0.05f), "hips on the bench top");
            Assert.That(player.transform.position.z - Yard.z, Is.InRange(BenchFront, BenchFront + BenchDepth), "hips on the bench");
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, 180f)), Is.LessThan(1f), "turned round to sit");
            yield return new WaitForSeconds(0.6f);
            Assert.That(eyes.position.y - (Yard.y + BenchTop), Is.EqualTo(player.seatEye).Within(0.05f), "eyes at sitting height");

            yield return Tap(keyboard.xKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Standing), "X again gets up");
            Assert.That(player.IsOnSeat, Is.False);
            Assert.That(body.enabled, Is.True);
            Assert.That(Vector3.Distance(player.transform.position, stood), Is.LessThan(0.1f), "you get up where you stood");
        }

        [UnityTest]
        public IEnumerator X_WhileStillHoldingW_StaysSeated_UntilYouLetGoAndWalkAgain()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(BenchLane, BenchFront - 0.5f);

            // walk into the bench with W held, then press X without letting go
            Press(keyboard.wKey);
            yield return new WaitForSeconds(0.3f);
            yield return Tap(keyboard.xKey);
            yield return new WaitForSeconds(0.3f);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Sitting), "a W that was already held doesn't get you up");
            Assert.That(player.IsOnSeat, Is.True);

            // let go, then a fresh W gets you up and walks you off
            Release(keyboard.wKey);
            yield return null;
            yield return Hold(keyboard.wKey, 0.3f);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Standing), "a fresh W gets you up");
        }

        [UnityTest]
        public IEnumerator X_SitsOnTheStepInFront_AtTheFootOfTheStairs()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(StairLane, StairStart - 0.5f);

            yield return Tap(keyboard.xKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Sitting), "X sits");
            Assert.That(player.IsOnSeat, Is.True, "on the first step, not the floor");
            Assert.That(player.transform.position.y - Yard.y, Is.EqualTo(StairRise).Within(0.05f), "on the first step");
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, 180f)), Is.LessThan(1f), "facing down the stairs");
        }

        [UnityTest]
        public IEnumerator X_LyingUnderADesk_DoesNotTeleportYouUpOntoIt()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(DeskLane, DeskStart - 2f);
            yield return Tap(keyboard.zKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "Z lies down");

            // lie under the desk facing along it, so its top is at seat height just in front of you
            yield return PutPlayerAt(DeskLane - 0.9f, DeskStart + DeskDepth * 0.5f);
            player.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            yield return null;

            yield return Tap(keyboard.xKey);
            Assert.That(player.IsOnSeat, Is.False, "never sit up through the desk");
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.LyingDown), "and no room to sit up under it either");
            Assert.That(player.transform.position.y - Yard.y, Is.LessThan(0.2f), "still on the floor");
        }

        [UnityTest]
        public IEnumerator X_SitsOnTheFloor_WhenThereIsNoSeat_AndWalkingGetsYouUp()
        {
            yield return LoadStarhouse();
            BuildYard();
            yield return PutPlayerAt(0f, 5f);
            Vector3 sat = player.transform.position;

            yield return Tap(keyboard.xKey);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Sitting), "X sits");
            Assert.That(player.IsOnSeat, Is.False, "on the floor");
            yield return new WaitForSeconds(0.6f);
            Assert.That(eyes.localPosition.y, Is.EqualTo(player.floorSitEye).Within(0.05f), "eyes at floor-sitting height");
            Assert.That(Flat(sat), Is.LessThan(0.05f), "sitting keeps you in place");

            yield return Hold(keyboard.wKey, 0.5f);
            Assert.That(player.CurrentStance, Is.EqualTo(Stance.Standing), "walking gets you up");
            Assert.That(Flat(sat), Is.GreaterThan(0.8f), "and you walk off");
        }

        // ---------------------------------------------------------------- the test yard

        static void BuildYard()
        {
            Block("Yard_Floor", Yard + new Vector3(0f, -0.25f, 0f), new Vector3(40f, 0.5f, 40f));
            Block("Yard_Bench", Yard + new Vector3(BenchLane, BenchTop * 0.5f, BenchFront + BenchDepth * 0.5f), new Vector3(2f, BenchTop, BenchDepth));
            Block("Yard_LowShelf", Yard + new Vector3(CrawlLane, ShelfBottom + 0.05f, ShelfStart + ShelfDepth * 0.5f), new Vector3(3f, 0.1f, ShelfDepth));
            Block("Yard_Table", Yard + new Vector3(TableLane, TableBottom + 0.05f, TableStart + TableDepth * 0.5f), new Vector3(3f, 0.1f, TableDepth));
            Block("Yard_Desk", Yard + new Vector3(DeskLane, (DeskBottom + DeskTop) * 0.5f, DeskStart + DeskDepth * 0.5f), new Vector3(3f, DeskTop - DeskBottom, DeskDepth));
            // solid stacked steps, like GB.Stairs, with a landing on top
            for (int i = 0; i < StairSteps; i++)
            {
                float top = (i + 1) * StairRise;
                Block($"Yard_Step{i}", Yard + new Vector3(StairLane, top * 0.5f, StairStart + (i + 0.5f) * StairRun), new Vector3(1.5f, top, StairRun));
            }
            Block("Yard_Landing", Yard + new Vector3(StairLane, StairSteps * StairRise * 0.5f, StairStart + StairSteps * StairRun + 1.5f), new Vector3(1.5f, StairSteps * StairRise, 3f));
        }

        static void Block(string name, Vector3 center, Vector3 size)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = center;
            block.transform.localScale = size;
        }

        /// Puts the player in the yard facing +Z and lets them settle on the floor.
        IEnumerator PutPlayerAt(float x, float z) => PutPlayer(Yard + new Vector3(x, 0f, z), 0f);
    }
}
