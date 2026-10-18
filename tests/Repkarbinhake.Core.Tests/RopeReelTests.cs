using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Tests.Fakes;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class RopeReelTests
    {
        private const int Precision = 4;
        private const float NoLoad = 0f;
        private const float FullStamina = 1f;
        private const int A = 1;
        private const int B = 2;

        private readonly TestSettings _settings = new TestSettings();
        private readonly RopeReel _reel;
        private readonly RopeLink _link = new RopeLink(A, B, 1L);

        public RopeReelTests()
        {
            _reel = new RopeReel(_settings, new RopeHandPolicy());
        }

        [Fact]
        public void LooseRope_HasFullLength()
        {
            Assert.Equal(_settings.MaxLength, _reel.EffectiveLength(_link), Precision);
        }

        [Fact]
        public void ReelOfReleasedEnd_IsIgnored()
        {
            _link.EndOf(A).Reel = 5f;

            Assert.Equal(_settings.MaxLength, _reel.EffectiveLength(_link), Precision);
        }

        [Fact]
        public void BothHeldEnds_ShortenTogether()
        {
            Hold(A, 3f);
            Hold(B, 4f);

            Assert.Equal(14f - 3f - 4f, _reel.EffectiveLength(_link), Precision);
        }

        [Fact]
        public void Length_FollowsLoweredHostMaxLength()
        {
            Hold(A, 2f);
            _settings.MaxLength = 8f;

            Assert.Equal(6f, _reel.EffectiveLength(_link), Precision);
        }

        [Fact]
        public void ReelIn_SlackRope_IsFullSpeedAndFree()
        {
            Hold(A, 0f);

            ReelStep step = ReelIn(0.5f, NoLoad, Footing.Grounded, FullStamina);

            Assert.Equal(6f * 0.5f, step.Reel, Precision);
            Assert.Equal(0f, step.StaminaCost);
        }

        [Fact]
        public void ReelOut_IsFreeEvenUnderLoad()
        {
            Hold(A, 5f);

            ReelStep step = _reel.StepReel(_link, A, 1f, 0.5f, 50f, Footing.Grounded, FullStamina);

            Assert.Equal(5f - 6f * 0.5f, step.Reel, Precision);
            Assert.Equal(0f, step.StaminaCost);
        }

        [Fact]
        public void ReelOut_StopsAtFullLength()
        {
            Hold(A, 1f);

            Assert.Equal(0f, _reel.StepReel(_link, A, 1f, 10f, NoLoad, Footing.Grounded, FullStamina).Reel, Precision);
        }

        [Fact]
        public void HangingPartner_MakesReelingInVerySlow()
        {
            Hold(A, 0f);

            float reeled = ReelIn(1f, 9.81f, Footing.Grounded, FullStamina).Reel;

            Assert.True(reeled > 0f);
            Assert.True(reeled < 6f * 0.25f);
        }

        [Fact]
        public void ReelIn_UnderLoad_CostsStaminaPerMeter()
        {
            Hold(A, 0f);

            float load = _settings.ArmStrength / 2f;

            ReelStep step = ReelIn(1f, load, Footing.Grounded, FullStamina);

            Assert.True(step.Reel > 0f);
            Assert.Equal(step.Reel * _reel.StaminaPerMeter(load), step.StaminaCost, Precision);
        }

        [Fact]
        public void HeavierLoad_CostsMorePerMeter()
        {
            Assert.True(_reel.StaminaPerMeter(9f) > _reel.StaminaPerMeter(3f));
            Assert.Equal(_settings.ReelStaminaPerMeter, _reel.StaminaPerMeter(100f), Precision);
        }

        [Fact]
        public void LowStamina_LimitsHowFarYouCanReel()
        {
            Hold(A, 0f);
            float stamina = 0.01f;

            float load = _settings.ArmStrength / 2f;

            ReelStep step = ReelIn(5f, load, Footing.Grounded, stamina);

            Assert.Equal(stamina / _reel.StaminaPerMeter(load), step.Reel, Precision);
            Assert.Equal(stamina, step.StaminaCost, Precision);
        }

        [Fact]
        public void ZeroStamina_UnderLoad_CannotReelIn()
        {
            Hold(A, 0f);

            ReelStep step = ReelIn(0.5f, 9.81f, Footing.Grounded, 0f);

            Assert.Equal(0f, step.Reel);
            Assert.Equal(0f, step.StaminaCost);
        }

        [Fact]
        public void ZeroStamina_SlackRope_CanStillReelIn()
        {
            Hold(A, 0f);

            Assert.True(ReelIn(0.5f, NoLoad, Footing.Grounded, 0f).Reel > 0f);
        }

        [Theory]
        [InlineData(Footing.Airborne)]
        [InlineData(Footing.Climbing)]
        [InlineData(Footing.Unconscious)]
        public void WithoutFooting_CannotReelIn(Footing footing)
        {
            Hold(A, 2f);

            ReelStep step = ReelIn(0.5f, NoLoad, footing, FullStamina);

            Assert.Equal(2f, step.Reel);
            Assert.Equal(0f, step.StaminaCost);
        }

        [Fact]
        public void Hanging_CanReelOutToLowerHimself()
        {
            Hold(A, 4f);

            Assert.True(_reel.StepReel(_link, A, 1f, 0.5f, 9.81f, Footing.Airborne, 0f).Reel < 4f);
        }

        [Fact]
        public void Hanging_ReelsOutSlowerThanGrounded()
        {
            Hold(A, 4f);

            float hanging = _reel.StepReel(_link, A, 1f, 0.5f, NoLoad, Footing.Airborne, FullStamina).Reel;

            Assert.Equal(4f - 6f * RopeReel.AirborneReelOutFactor * 0.5f, hanging, Precision);
        }

        [Fact]
        public void Climbing_CannotReelOut()
        {
            Hold(A, 4f);

            Assert.Equal(4f, _reel.StepReel(_link, A, 1f, 0.5f, NoLoad, Footing.Climbing, FullStamina).Reel);
        }

        [Fact]
        public void ReelIn_LeavesRoomForPartnerReel()
        {
            Hold(A, 0f);
            Hold(B, 10f);

            float reel = ReelIn(50f, NoLoad, Footing.Grounded, FullStamina).Reel;

            Assert.Equal(14f - 1.5f - 10f, reel, Precision);
            _link.EndOf(A).Reel = reel;
            Assert.Equal(_settings.MinLength, _reel.EffectiveLength(_link), Precision);
        }

        [Fact]
        public void PartnerAlreadyAtMinimum_LeavesNothingToReel()
        {
            Hold(A, 0f);
            Hold(B, 14f - 1.5f);

            Assert.Equal(0f, ReelIn(0.5f, NoLoad, Footing.Grounded, FullStamina).Reel, Precision);
        }

        [Fact]
        public void ReelForLength_AccountsForPartner()
        {
            Hold(B, 4f);

            Assert.Equal(14f - 4f - 5f, _reel.ReelForLength(_link, A, 5f), Precision);
        }

        [Theory]
        [InlineData(12f)]
        [InlineData(100f)]
        public void LoadAtOrAboveArmStrength_StillReelsInAtMinimumSpeed(float load)
        {
            Hold(A, 0f);

            Assert.Equal(RopeReel.MinReelInFactor, _reel.ReelInFactor(load), Precision);
            Assert.True(ReelIn(1f, load, Footing.Grounded, FullStamina).Reel > 0f);
        }

        [Fact]
        public void ReelInSpeed_FallsLinearlyWithLoad()
        {
            Assert.Equal(1f, _reel.ReelInFactor(0f), Precision);
            Assert.Equal(0.5f, _reel.ReelInFactor(_settings.ArmStrength / 2f), Precision);
        }

        [Fact]
        public void ZeroArmStrength_ReelsOnlySlackRope()
        {
            _settings.ArmStrength = 0f;

            Assert.Equal(0f, _reel.ReelInFactor(1f), Precision);
            Assert.Equal(1f, _reel.ReelInFactor(0f), Precision);
        }

        private ReelStep ReelIn(float deltaTime, float load, Footing footing, float stamina)
        {
            return _reel.StepReel(_link, A, -1f, deltaTime, load, footing, stamina);
        }

        private void Hold(int actor, float reel)
        {
            RopeEnd end = _link.EndOf(actor);
            end.IsHeld = true;
            end.Reel = reel;
        }
    }
}
