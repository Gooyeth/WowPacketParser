﻿using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V6_0_2_19033.Parsers
{
    public static class TicketHandler
    {
        [Parser(Opcode.CMSG_GM_TICKET_GET_TICKET)]
        [Parser(Opcode.CMSG_GM_TICKET_GET_CASE_STATUS)]
        [Parser(Opcode.CMSG_GM_TICKET_GET_SYSTEM_STATUS)]
        [Parser(Opcode.SMSG_GM_TICKET_RESPONSE_ERROR)]
        public static void HandleGMTicketZero(Packet packet)
        {
        }

        [Parser(Opcode.SMSG_GM_TICKET_CASE_STATUS)]
        public static void HandleGMTicketCaseStatus(Packet packet)
        {
            packet.ReadTime("OldestTicketTime");
            packet.ReadTime("UpdateTime");

            var int24 = packet.ReadInt32("CasesCount");
            for (int i = 0; i < int24; i++)
            {
                packet.ReadInt32("CaseID", i);
                packet.ReadInt32("CaseOpened", i);
                packet.ReadInt32("CaseStatus", i);
                packet.ReadInt16("CfgRealmID", i);
                packet.ReadInt64("CharacterID", i);
                packet.ReadInt32("WaitTimeOverrideMinutes", i);

                packet.ResetBitReader();
                var bits12 = packet.ReadBits(11);
                var bits262 = packet.ReadBits(10);

                packet.ReadWoWString("Url", bits12, i);
                packet.ReadWoWString("WaitTimeOverrideMessage", bits262, i);
            }
        }

        [Parser(Opcode.SMSG_GM_TICKET_SYSTEM_STATUS)]
        public static void HandleGMTicketSystemStatus(Packet packet)
        {
            packet.ReadInt32("Status");
        }

        [Parser(Opcode.SMSG_GM_TICKET_GET_TICKET_RESPONSE)]
        public static void HandleGMTicketGetTicketResponse(Packet packet)
        {
            packet.ReadInt32("Result");

            var bit3068 = packet.ReadBit("HasInfo");

            // ClientGMTicketInfo
            if (bit3068)
            {
                packet.ReadInt32("TicketID");
                packet.ReadByte("Category");
                packet.ReadTime("TicketOpenTime");
                packet.ReadTime("OldestTicketTime");
                packet.ReadTime("UpdateTime");
                packet.ReadByte("AssignedToGM");
                packet.ReadByte("OpenedByGM");
                packet.ReadInt32("WaitTimeOverrideMinutes");

                packet.ResetBitReader();

                var bits1 = packet.ReadBits(11);
                var bits2022 = packet.ReadBits(10);

                packet.ReadWoWString("TicketDescription", bits1);
                packet.ReadWoWString("WaitTimeOverrideMessage", bits2022);
            }
        }

        [Parser(Opcode.CMSG_COMPLAIN)]
        public static void HandleComplain(Packet packet)
        {
            var result = packet.ReadByte("Offender");

            if (result == 0)
                packet.ReadInt32("MailID");

            if (result == 1)
            {
                packet.ReadInt32("Command");
                packet.ReadInt32("ChannelID");

                packet.ResetBitReader();

                var len = packet.ReadBits(12);
                packet.ReadWoWString("MessageLog", len);
            }

            if (result == 2)
            {
                // Order guessed
                packet.ReadInt64("EventGuid");
                packet.ReadInt64("InviteGuid");
            }
        }

        [Parser(Opcode.CMSG_GM_TICKET_ACKNOWLEDGE_SURVEY)]
        public static void HandleGMTicketAcknowledgeSurvey(Packet packet)
        {
            packet.ReadInt32("CaseID");
        }

        [Parser(Opcode.CMSG_GM_TICKET_CREATE)]
        public static void HandleGMTicketCreate(Packet packet)
        {
            // TODO: confirm order, test
            packet.ReadInt32<MapId>("Map");
            packet.ReadVector3("Pos");
            packet.ReadByte("Flags");

            var descriptionLength = packet.ReadBits("DescriptionLength", 11);
            packet.ResetBitReader();
            packet.ReadWoWString("Description", descriptionLength);

            packet.ReadBit("NeedMoreHelp");
            packet.ReadBit("NeedResponse");
            packet.ResetBitReader();

            var dataCount = packet.ReadInt32("ChatHistoryDataCount");
            var pkt = packet.Inflate(dataCount);
            pkt.ReadCString("ChatHistoryData");
            pkt.ClosePacket(false);
        }

        [Parser(Opcode.CMSG_GM_TICKET_UPDATE_TEXT)]
        public static void HandleGMTicketUpdatetext(Packet packet)
        {
            var length = packet.ReadBits("DescriptionLength", 11);
            packet.ResetBitReader();

            packet.ReadWoWString("Description", length);
        }

        public static void ReadClientGMSurveyQuestion(Packet packet, params object[] idx)
        {
            packet.ReadInt32("QuestionID", idx);
            packet.ReadByte("Answer", idx);

            packet.ResetBitReader();
            var length = packet.ReadBits("AnswerCommentLength", 11, idx);
            packet.ResetBitReader();

            packet.ReadWoWString("AnswerComment", length, idx);
        }

        [Parser(Opcode.CMSG_GM_SURVEY_SUBMIT)]
        public static void HandleGMSurveySubmit(Packet packet)
        {
            packet.ReadInt32("SurveyID");

            var questionCount = packet.ReadBits("SurveyQuestionCount", 4);
            var commentLenght = packet.ReadBits("CommentLength", 11);

            packet.ResetBitReader();

            for (var i = 0; i < questionCount; ++i)
                ReadClientGMSurveyQuestion(packet, "SurveyQuestion", i);

            packet.ReadWoWString("Comment", commentLenght);
        }
    }
}
