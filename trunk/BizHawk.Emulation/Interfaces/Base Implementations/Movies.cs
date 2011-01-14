﻿using System.IO;

namespace BizHawk
{
    public class InputRecorder : IController
    {
        private IController baseController;
        private BinaryWriter writer;

        public InputRecorder(IController baseController, BinaryWriter writer)
        {
            this.baseController = baseController;
            this.writer = writer;
        }

        public void CloseMovie()
        {
            writer.Close();
        }

        public ControllerDefinition Type
        {
            get { return baseController.Type; }
        }

        public bool this[string button]
        {
            get { return baseController[button]; }
        }

        public bool IsPressed(string button)
        {
            return baseController[button];
        }

        public float GetFloat(string name)
        {
            return baseController.GetFloat(name);
        }

        public void UnpressButton(string name)
        {
            baseController.UnpressButton(name);
        }

        public void ForceButton(string button)
        {
            baseController.ForceButton(button);
        }

        private int frame;
        public int FrameNumber
        {
            get { return frame; }
            set
            {
                if (frame != value)
                {
                    frame = value;
                    RecordFrame();
                } 
                baseController.FrameNumber = value;
            }
        }

        private void RecordFrame()
        {
            int encodedValue = 0;
            for (int i=0; i<Type.BoolButtons.Count; i++)
            {
                if (baseController[Type.BoolButtons[i]])
                {
                    encodedValue |= (1 << i);
                }
            }
            writer.Seek(frame*2, SeekOrigin.Begin);
            writer.Write((ushort)encodedValue);
        }

        public void SetSticky(string button, bool sticky)
        {
            baseController.SetSticky(button, sticky);
        }

        public bool IsSticky(string button)
        {
            return baseController.IsSticky(button);
        }
    }

    public class InputPlayback : IController
    {
        private ControllerDefinition def;
        private int[] input;

        public InputPlayback(ControllerDefinition controllerDefinition, BinaryReader reader)
        {
            def = controllerDefinition;
            int numFrames = (int) (reader.BaseStream.Length/2);
            input = new int[numFrames];
            for (int i=0; i<numFrames; i++)
                input[i] = reader.ReadUInt16();
        }

        public ControllerDefinition Type
        {
            get { return def; }
        }

        public bool this[string button]
        {
            get { return IsPressed(button); }
        }

        public bool IsPressed(string button)
        {
            if (FrameNumber >= input.Length)
                return false;

            for (int i = 0; i < def.BoolButtons.Count; i++)
            {
                if (def.BoolButtons[i] == button)
                {
                    return (input[FrameNumber] & (1 << i)) != 0;
                }
            }
            return false;
        }

        public float GetFloat(string name)
        {
            throw new System.NotImplementedException();
        }

        public void UnpressButton(string name) {}
        public void ForceButton(string button) { }
        public void SetSticky(string button, bool sticky) { }
        public bool IsSticky(string button) { return false; }
        public int FrameNumber { get; set; }

        public bool MovieEnded { get { return FrameNumber >= input.Length; } }
    }
}