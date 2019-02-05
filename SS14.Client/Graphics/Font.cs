﻿using System;
using SS14.Client.Graphics.Drawing;
using SS14.Client.Interfaces.Graphics;
using SS14.Client.ResourceManagement;
using SS14.Shared.IoC;
using SS14.Shared.Maths;
using SS14.Shared.Utility;

namespace SS14.Client.Graphics
{
    /// <summary>
    ///     A generic font for rendering of text.
    ///     Does not contain properties such as size. Those are specific to children such as <see cref="VectorFont" />
    /// </summary>
    public abstract class Font
    {
        internal abstract Godot.Font GodotFont { get; }

        public virtual int Ascent => (int?)GodotFont?.GetAscent() ?? default;
        public virtual int Height => (int?)GodotFont?.GetHeight() ?? default;
        public virtual int Descent => (int?)GodotFont?.GetDescent() ?? default;

        public static implicit operator Godot.Font(Font font)
        {
            return font?.GodotFont;
        }

        // Yes, I am aware that using char is bad.
        // At the same time the font system is nowhere close to rendering Unicode so...
        /// <summary>
        ///     Draw a character at a certain baseline position on screen.
        /// </summary>
        /// <param name="handle">The drawing handle to draw to.</param>
        /// <param name="chr">
        ///     The Unicode code point to draw. Yes I'm aware about UTF-16 being crap,
        ///     do you think this system can draw anything except ASCII?
        /// </param>
        /// <param name="baseline">The baseline from which to draw the character.</param>
        /// <param name="color">The color of the character to draw.</param>
        /// <returns>How much to advance the cursor to draw the next character.</returns>
        public abstract float DrawChar(DrawingHandleScreen handle, char chr, Vector2 baseline, Color color);

        public abstract CharMetrics? GetCharMetrics(char chr);
    }

    /// <summary>
    ///     Font type that renders vector fonts such as OTF and TTF fonts from a <see cref="FontResource"/>
    /// </summary>
    public sealed class VectorFont : Font
    {
        public int Size { get; }

        internal override Godot.Font GodotFont => _font;

        private readonly Godot.DynamicFont _font;

        internal IFontInstanceHandle Handle { get; }

        public override int Ascent => Handle?.Ascent ?? base.Ascent;
        public override int Descent => Handle?.Descent ?? base.Descent;
        public override int Height => Handle?.Height ?? base.Height;

        public VectorFont(FontResource res, int size)
        {
            Size = size;
            if (GameController.OnGodot)
            {
                _font = new Godot.DynamicFont
                {
                    FontData = res.FontData,
                    Size = size,
                };
            }
            else
            {
                Handle = IoCManager.Resolve<IFontManagerInternal>().MakeInstance(res.FontFaceHandle, size);
            }
        }

        public override float DrawChar(DrawingHandleScreen handle, char chr, Vector2 baseline, Color color)
        {
            DebugTools.Assert(!GameController.OnGodot);

            var texture = Handle.GetCharTexture(chr);
            if (texture == null)
            {
                return 0;
            }
            var metrics = Handle.GetCharMetrics(chr);
            if (!metrics.HasValue)
            {
                return 0;
            }

            baseline += new Vector2(metrics.Value.BearingX, -metrics.Value.BearingY);
            handle.DrawTexture(texture, baseline, color);
            return metrics.Value.Advance;
        }

        public override CharMetrics? GetCharMetrics(char chr)
        {
            return Handle.GetCharMetrics(chr);
        }
    }

    internal sealed class GodotWrapFont : Font
    {
        public GodotWrapFont(Godot.Font godotFont)
        {
            GodotFont = godotFont;
        }

        internal override Godot.Font GodotFont { get; }

        public override float DrawChar(DrawingHandleScreen handle, char chr, Vector2 baseline, Color color)
        {
            throw new NotImplementedException();
        }

        public override CharMetrics? GetCharMetrics(char chr)
        {
            throw new NotImplementedException();
        }
    }
}
