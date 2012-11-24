using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProtoBuf;

namespace Protocol
{
    public static class ProtocolUtility
    {
        private static Dictionary<Type, int> m_packetInfo = new Dictionary<Type, int>();
        private static Dictionary<int, Type> m_packetTypes = new Dictionary<int,Type>();

        static ProtocolUtility()
        {
            Type[] types = Assembly.GetAssembly(typeof(ProtocolUtility)).GetTypes();
            IEnumerable<Type> protocolTypes = types.Where(t => t.GetCustomAttributes(typeof(ProtoContractAttribute), false).Any());
            foreach (Type t in protocolTypes)
            {
                int packetTypeCode = Math.Abs(CalculatePacketTypeCode(t));
                m_packetInfo.Add(t, packetTypeCode);
                m_packetTypes.Add(packetTypeCode, t);
            }
        }

        private static int CalculatePacketTypeCode(Type t)
        {
            return (int)CRC32.Compute(Encoding.ASCII.GetBytes(t.Name));
        }

        public static Type GetPacketType(int typeCode)
        {
            Type t = null;
            m_packetTypes.TryGetValue(typeCode, out t);
            return t;
        }

        public static int? GetPacketTypeCode(Type t)
        {
            int code;
            if (m_packetInfo.TryGetValue(t, out code))
            {
                return code;
            }
            return null;
        }
    }
}
