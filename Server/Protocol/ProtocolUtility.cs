using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Protocol
{
    public static class ProtocolUtility
    {
        private static Dictionary<Type, int> m_typeToCode = new Dictionary<Type, int>();
        private static Dictionary<int, Type> m_codeToType = new Dictionary<int, Type>();

        private static bool m_isInitialised = false;

        static ProtocolUtility()
        {
            Type[] types = Assembly.GetAssembly(typeof(ProtocolUtility)).GetTypes();
            IEnumerable<Type> protocolTypes = types.Where(t => t.GetCustomAttributes(typeof(ProtoContractAttribute), false).Any());
            int i = 1;
            foreach (Type t in protocolTypes.OrderBy(t => t.Name))
            {
                int packetTypeCode = i++;
                m_typeToCode.Add(t, packetTypeCode);
                m_codeToType.Add(packetTypeCode, t);
            }
        }

        public static Type GetPacketType(int typeCode)
        {
            Type t = null;
            m_codeToType.TryGetValue(typeCode, out t);
            return t;
        }

        public static int? GetPacketTypeCode(Type t)
        {
            int code;
            if (m_typeToCode.TryGetValue(t, out code))
            {
                return code;
            }
            return null;
        }

        public static void InitialiseSerializer()
        {
            if (!m_isInitialised)
            {
                m_isInitialised = true;

                Type packetType = typeof(Packet);
                Assembly protocolAssembly = Assembly.GetAssembly(packetType);
                Type[] types = protocolAssembly.GetTypes();

                MetaType metaType = RuntimeTypeModel.Default.Add(packetType, true);

                //Count how many ProtoMembers Packet has and start tagging from there
                int startTag = packetType.GetProperties().Where(p => p.GetCustomAttributes(typeof(ProtoMemberAttribute), false).Count() > 0).Count() + 1;

                foreach (Type packetSubclass in types.Where(t => t.IsSubclassOf(packetType)))
                {
                    metaType.AddSubType(startTag++, packetSubclass);
                }

                RuntimeTypeModel.Default.CompileInPlace();
                RuntimeTypeModel.Default.AutoCompile = true;
            }
        }
    }
}
