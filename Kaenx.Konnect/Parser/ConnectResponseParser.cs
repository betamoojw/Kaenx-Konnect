﻿using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Parser
{
    class ConnectResponseParser : IReceiveParser
    {
        public ushort ServiceTypeIdentifier => 0x0206;

        IParserMessage IReceiveParser.Build(byte headerLength, byte protocolVersion, ushort totalLength,
          byte[] responseBytes)
        {
            return Build(headerLength, protocolVersion, totalLength, responseBytes);
        }

        public ConnectResponse Build(byte headerLength, byte protocolVersion, ushort totalLength, byte[] responseBytes)
        {
            var communicationChannel = responseBytes[0];
            var status = responseBytes[1];
            var dataEndpoint = ParseEndpoint(responseBytes.Skip(2).Take(8));
            var connectionResponseDataBlock = ParseConnectionResponseDataBlock(responseBytes.Skip(10).Take(4));

            return new ConnectResponse(headerLength, protocolVersion, totalLength, communicationChannel, status, dataEndpoint,
              connectionResponseDataBlock);
        }

        private static ConnectionResponseDataBlock ParseConnectionResponseDataBlock(IEnumerable<byte> bytes)
        {
            if (bytes.Count() == 0) return new ConnectionResponseDataBlock(0x00, 0x01, UnicastAddress.FromString("0.0.0"));

            var enumerable = bytes as byte[] ?? bytes.ToArray();

            if(enumerable.ElementAt(1) == 0x04)
            {
              //Wenn Tunneling
              return new ConnectionResponseDataBlock(enumerable.ElementAt(0), enumerable.ElementAt(1),
                UnicastAddress.FromByteArray(enumerable.Skip(2).ToArray()));
            } else {
              //Wenn Config
              return new ConnectionResponseDataBlock(enumerable.ElementAt(0), enumerable.ElementAt(1),
                UnicastAddress.FromString("0.0.0"));
            }
        }

        private static HostProtocolAddressInformation ParseEndpoint(IEnumerable<byte> bytes)
        {
            if(bytes.Count() == 0) return new HostProtocolAddressInformation(0x01,
              new IPEndPoint(IPAddress.Any, 0));


            var enumerable = bytes as byte[] ?? bytes.ToArray();
            return new HostProtocolAddressInformation(enumerable.ElementAt(1),
              new IPEndPoint(new IPAddress(enumerable.Skip(2).Take(4).ToArray()),
                BitConverter.ToInt16(enumerable.Skip(6).Take(2).Reverse().ToArray(), 0)));
        }
    }
}
