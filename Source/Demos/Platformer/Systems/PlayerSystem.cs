using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Input;
using Platformer.Collisions;
using Platformer.Components;

namespace Platformer.Systems
{
    [Aspect(AspectType.All, typeof(Body), typeof(Player), typeof(Transform2), typeof(AnimatedSprite))]
    [EntitySystem(GameLoopType.Update, Layer = 0)]
    public class PlayerSystem : EntityProcessingSystem
    {
        protected override void Process(GameTime gameTime, Entity entity)
        {
            var player = entity.Get<Player>();
            var sprite = entity.Get<AnimatedSprite>();
            var transform = entity.Get<Transform2>();
            var body = entity.Get<Body>();
            var keyboardState = KeyboardExtended.GetState();
            
            if (keyboardState.WasKeyJustUp(Keys.Up))
            {
                if (player.State != State.Jumping && player.State != State.Falling && player.State != State.Attacking)
                    body.Velocity.Y -= 550 + Math.Abs(body.Velocity.X) * 0.4f;

                player.State = State.Jumping;
            }

            if (keyboardState.WasKeyJustUp(Keys.Z))
            {
                if(player.State == State.Idle)
                    body.Velocity.Y -= 550 + Math.Abs(body.Velocity.X) * 0.4f;

                player.State = State.Attacking;
            }
            
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                body.Velocity.X += 150;
                player.Facing = Facing.Right;
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                body.Velocity.X -= 150;
                player.Facing = Facing.Left;
            }

            if (body.Velocity.Y < 0)
                player.State = State.Jumping;

            if (body.Velocity.Y > 0)
                player.State = State.Falling;

            if (player.State != State.Attacking)
            {
                if (body.Velocity.X > 0 || body.Velocity.X < 0)
                    player.State = State.Walking;
                
                if (body.Velocity.EqualsWithTolerence(Vector2.Zero, 5f))
                    player.State = State.Idle;
            }

            if (keyboardState.IsKeyDown(Keys.Down))
                player.State = State.Cool;

            switch (player.State)
            {
                case State.Jumping:
                    sprite.Play("jump");
                    break;
                case State.Walking:
                    sprite.Play("walk");
                    sprite.Effect = player.Facing == Facing.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    break;
                case State.Falling:
                    sprite.Play("fall");
                    break;
                case State.Idle:
                    sprite.Play("idle");
                    break;
                case State.Attacking:
                    sprite.Play("kick", () => player.State = State.Idle);
                    break;
                case State.Cool:
                    sprite.Play("cool");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            body.Velocity.X *= 0.7f;

            // TODO: Can we remove this?
            transform.Position = body.Position;
        }
    }
}