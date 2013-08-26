
namespace Protocol
{
    public static class Compression
    {
        private const float RAD2DEG = 57.2957795f;
        private const float DEG2RAD = 0.0174532925f;

        /// <summary>
        /// Convert a rotation in the range [0..2π] to a byte
        /// </summary>
        public static byte RotationToByte(float rotation)
        {
            int degrees = (int)(RAD2DEG * rotation) % 360;

            return (byte)(degrees * (255f / 360));
        }

        public static short VelocityToShort(float velocity)
        {
            return (short)(velocity * 100);
        }

        public static ushort PositionToUShort(float position)
        {
            return (ushort)(position * 10);
        }

        public static float ByteToRotation(byte rotation)
        {
            return DEG2RAD * rotation;
        }

        public static float ShortToVelocity(short velocity)
        {
            return (float)velocity * 0.01f;
        }

        public static float UShortToPosition(ushort position)
        {
            return (float)position * 0.1f;
        }
    }
}
