/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using VoluntLib2.ComputationLayer;

namespace VoluntLib2.ManagementLayer
{    
    public class Job : IEquatable<Job>
    {
        private const int STRING_MAX_LENGTH = 255;
        private const int STRING_MAX_JOB_DESCRIPTION_LENGTH = 1024; //1kb

        public Job(BigInteger jobID)
        {
            JobID = jobID;
            JobName = string.Empty;
            JobType = string.Empty;
            JobDescription = string.Empty;
            WorldName = string.Empty;
            Creator = string.Empty;
            NumberOfBlocks = BigInteger.Zero;
            IsDeleted = false;
            JobPayload = new byte[0];
        }

        public BigInteger JobID { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string JobDescription { get; set; }
        public string WorldName { get; set; }
        public string Creator { get; set; }
        public BigInteger NumberOfBlocks { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreationDate { get; set; }
        public byte[] JobPayload { get; set; }       

        public bool Equals(Job other)
        {
            return other.JobID.Equals(JobID);
        }

        public bool HasPayload()
        {
            return JobPayload != null && JobPayload.Length > 0;
        }

        public override int GetHashCode()
        {
            return JobID.GetHashCode();
        }

        public byte[] Serialize()
        {
            //0. Trim string length to max lengths
            if (JobName.Length > STRING_MAX_LENGTH)
            {
                JobName = JobName.Substring(0, STRING_MAX_LENGTH);
            }
            if (JobType.Length > STRING_MAX_LENGTH)
            {
                JobType = JobType.Substring(0, STRING_MAX_LENGTH);
            }
            if (JobDescription.Length > STRING_MAX_JOB_DESCRIPTION_LENGTH)
            {
                JobDescription = JobDescription.Substring(0, STRING_MAX_LENGTH);
            }
            if (WorldName.Length > STRING_MAX_LENGTH)
            {
                WorldName = WorldName.Substring(0, STRING_MAX_LENGTH);
            }
            if (Creator.Length > STRING_MAX_LENGTH)
            {
                Creator = Creator.Substring(0, STRING_MAX_LENGTH);
            }

            //1. Convert all to byte arrays + "length of fields"-byte arrays; also calculate total size of data array
            int length = 0;  
            byte[] jobIdBytes = JobID.ToByteArray();            
            byte[] jobIdLength = BitConverter.GetBytes((ushort)jobIdBytes.Length);
            length += (jobIdBytes.Length + jobIdLength.Length);

            byte[] jobNameBytes = UTF8Encoding.UTF8.GetBytes(JobName);
            byte[] jobNameLength = BitConverter.GetBytes((ushort)jobNameBytes.Length);
            length += (jobNameBytes.Length + jobNameLength.Length);

            byte[] jobTypeBytes = UTF8Encoding.UTF8.GetBytes(JobType);
            byte[] jobTypeLength = BitConverter.GetBytes((ushort)jobTypeBytes.Length);
            length += (jobTypeBytes.Length + jobTypeLength.Length);

            byte[] jobDescriptionBytes = UTF8Encoding.UTF8.GetBytes(JobDescription);
            byte[] jobDescriptionLength = BitConverter.GetBytes((ushort)jobDescriptionBytes.Length);
            length += (jobDescriptionBytes.Length + jobDescriptionLength.Length);

            byte[] worldNameBytes = UTF8Encoding.UTF8.GetBytes(WorldName);
            byte[] worldNameLength = BitConverter.GetBytes((ushort)worldNameBytes.Length);
            length += (worldNameBytes.Length + worldNameLength.Length);

            byte[] creatorBytes = UTF8Encoding.UTF8.GetBytes(Creator);
            byte[] creatorLength = BitConverter.GetBytes((ushort)creatorBytes.Length);
            length += (creatorBytes.Length + creatorLength.Length);

            byte[] numberOfBlocksBytes = NumberOfBlocks.ToByteArray();
            byte[] numberOfBlocksLength = BitConverter.GetBytes((ushort)numberOfBlocksBytes.Length);
            length += (numberOfBlocksBytes.Length + numberOfBlocksLength.Length);

            byte isDeleted = (byte)(IsDeleted == true ? 0 : 1);
            length += 1;

            byte[] creationDateBytes = BitConverter.GetBytes(CreationDate.ToBinary());            
            length += 8;

            byte[] jobPayloadLength = BitConverter.GetBytes((ushort)JobPayload.Length);
            length += (jobPayloadLength.Length + JobPayload.Length);

            //2. Generate final array using length; copy everyhting into array
            byte[] data = new byte[length];

            int offset = 0;
            Array.Copy(jobIdLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(jobIdBytes, 0, data, offset, jobIdBytes.Length);
            offset += jobIdBytes.Length;

            Array.Copy(jobNameLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(jobNameBytes, 0, data, offset, jobNameBytes.Length);
            offset += jobNameBytes.Length;

            Array.Copy(jobTypeLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(jobTypeBytes, 0, data, offset, jobTypeBytes.Length);
            offset += jobTypeBytes.Length;

            Array.Copy(jobDescriptionLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(jobDescriptionBytes, 0, data, offset, jobDescriptionBytes.Length);
            offset += jobDescriptionBytes.Length;

            Array.Copy(worldNameLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(worldNameBytes, 0, data, offset, worldNameBytes.Length);
            offset += worldNameBytes.Length;

            Array.Copy(creatorLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(creatorBytes, 0, data, offset, creatorBytes.Length);
            offset += creatorBytes.Length;

            Array.Copy(numberOfBlocksLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(numberOfBlocksBytes, 0, data, offset, numberOfBlocksBytes.Length);
            offset += numberOfBlocksBytes.Length;

            data[offset] = isDeleted;
            offset += 1;

            Array.Copy(creationDateBytes, 0, data, offset, creationDateBytes.Length);
            offset += 8;

            Array.Copy(jobPayloadLength, 0, data, offset, 2);            
            offset += 2;
            Array.Copy(JobPayload, 0, data, offset, JobPayload.Length);

            return data;
        }

        public void Deserialize(byte[] data)
        {
            int offset = 0;

            ushort jobIdLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            byte[] jobId = new byte[jobIdLength];
            Array.Copy(data, offset, jobId, 0, jobIdLength);
            JobID = new BigInteger(jobId);
            offset += jobIdLength;

            ushort jobNameLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobName = UTF8Encoding.UTF8.GetString(data, offset, jobNameLength);
            offset += jobNameLength;

            ushort jobTypeLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobType = UTF8Encoding.UTF8.GetString(data, offset, jobTypeLength);
            offset += jobTypeLength;

            ushort jobDescriptionLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobDescription = UTF8Encoding.UTF8.GetString(data, offset, jobDescriptionLength);
            offset += jobDescriptionLength;

            ushort worldNameLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            WorldName = UTF8Encoding.UTF8.GetString(data, offset, worldNameLength);
            offset += worldNameLength;

            ushort creatorLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            Creator = UTF8Encoding.UTF8.GetString(data, offset, creatorLength);
            offset += creatorLength;

            ushort numberOfBlocksLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            byte[] numberOfBlocks = new byte[numberOfBlocksLength];
            Array.Copy(data, offset, numberOfBlocks, 0, numberOfBlocksLength);
            NumberOfBlocks = new BigInteger(numberOfBlocks);
            offset += numberOfBlocksLength;

            IsDeleted = data[offset] == 0;
            offset += 1;

            CreationDate = DateTime.FromBinary(BitConverter.ToInt64(data, offset));
            offset += 8;

            ushort jobPayloadLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobPayload = new byte[jobPayloadLength];
            Array.Copy(data, offset, JobPayload, 0, jobPayloadLength);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Job");
            builder.AppendLine("{");
            builder.Append("  JobID: ");
            builder.AppendLine(JobID + ",");
            builder.Append("  JobName: ");
            builder.AppendLine(JobName + ",");
            builder.Append("  JobType: ");
            builder.AppendLine("" + JobType + ",");
            builder.Append("  JobDescription: ");
            builder.AppendLine("" + JobDescription + ",");
            builder.Append("  WorldName: ");
            builder.AppendLine("" + WorldName + ",");
            builder.Append("  Creator: ");
            builder.AppendLine("" + Creator + ",");
            builder.Append("  NumberOfBlocks: ");
            builder.AppendLine("" + NumberOfBlocks + ",");
            builder.Append("  IsDeleted: ");
            builder.AppendLine("" + IsDeleted + ",");
            builder.Append("  HasPayload: ");
            builder.AppendLine("" + HasPayload() + ",");
            builder.Append("  JobPayload: ");
            builder.AppendLine("" + BitConverter.ToString(JobPayload));
            builder.AppendLine("}");

            return builder.ToString();
        }        
    }
}
