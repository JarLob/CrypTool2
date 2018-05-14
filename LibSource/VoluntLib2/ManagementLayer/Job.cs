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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VoluntLib2.ComputationLayer;
using VoluntLib2.Tools;

namespace VoluntLib2.ManagementLayer
{    
    public class Job : IEquatable<Job>, IComparable<Job>
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
            CreatorName = string.Empty;
            NumberOfBlocks = BigInteger.Zero;                        
            CreatorCertificateData = new byte[0];
            JobPayloadHash = new byte[0];
            JobCreationSignatureData = new byte[0];
            JobDeletionSignatureData = new byte[0];
            JobPayload = new byte[0];
            LastPayloadRequestTime = DateTime.MinValue;
        }

        public BigInteger JobID { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string JobDescription { get; set; }
        public string WorldName { get; set; }
        public string CreatorName { get; set; }
        public BigInteger NumberOfBlocks { get; set; }
        public DateTime CreationDate { get; set; }

        public DateTime LastPayloadRequestTime { get; set; }

        public byte[] CreatorCertificateData { get; set; }
        public byte[] JobPayloadHash { get; set; }
        public byte[] JobCreationSignatureData { get; set; }
        public byte[] JobDeletionSignatureData { get; set; }
        public byte[] JobPayload { get; set; }
        
        public long JobSize
        {
            get
            {
                long size = 0;
                size += JobID.ToByteArray().Length;
                size += UTF8Encoding.UTF8.GetBytes(JobName).Length;
                size += UTF8Encoding.UTF8.GetBytes(JobType).Length;
                size += UTF8Encoding.UTF8.GetBytes(JobDescription).Length;
                size += UTF8Encoding.UTF8.GetBytes(WorldName).Length;
                size += UTF8Encoding.UTF8.GetBytes(CreatorName).Length;
                size += NumberOfBlocks.ToByteArray().Length;
                size += 8; //CreationDate
                size += CreatorCertificateData != null ? CreatorCertificateData.Length : 0;
                size += JobPayloadHash != null ? JobPayloadHash.Length : 0;
                size += JobCreationSignatureData != null ? JobCreationSignatureData.Length : 0;
                size += JobDeletionSignatureData != null ? JobDeletionSignatureData.Length : 0;
                size += JobPayload != null ? JobPayload.Length : 0;
                return size;
            }
        }

        public bool IsDeleted {

            get { return false; }
        
        }

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

        /// <summary>
        /// Serializes the job to a byte array
        /// </summary>
        /// <returns></returns>
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
            if (CreatorName.Length > STRING_MAX_LENGTH)
            {
                CreatorName = CreatorName.Substring(0, STRING_MAX_LENGTH);
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

            byte[] creatorBytes = UTF8Encoding.UTF8.GetBytes(CreatorName);
            byte[] creatorLength = BitConverter.GetBytes((ushort)creatorBytes.Length);
            length += (creatorBytes.Length + creatorLength.Length);

            byte[] numberOfBlocksBytes = NumberOfBlocks.ToByteArray();
            byte[] numberOfBlocksLength = BitConverter.GetBytes((ushort)numberOfBlocksBytes.Length);
            length += (numberOfBlocksBytes.Length + numberOfBlocksLength.Length);

            byte[] creationDateBytes = BitConverter.GetBytes(CreationDate.ToBinary());            
            length += 8;

            byte[] creatorCertificateDataLength = BitConverter.GetBytes((ushort)CreatorCertificateData.Length);
            length += (creatorCertificateDataLength.Length + CreatorCertificateData.Length);

            byte[] jobPayloadHashLength = BitConverter.GetBytes((ushort)JobPayloadHash.Length);
            length += (jobPayloadHashLength.Length + JobPayloadHash.Length);

            byte[] jobCreationSignatureDataLength = BitConverter.GetBytes((ushort)JobCreationSignatureData.Length);
            length += (jobCreationSignatureDataLength.Length + JobCreationSignatureData.Length);

            byte[] jobDeletionSignatureDataLength = BitConverter.GetBytes((ushort)JobDeletionSignatureData.Length);
            length += (jobDeletionSignatureDataLength.Length + JobDeletionSignatureData.Length);
        
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

            Array.Copy(creationDateBytes, 0, data, offset, creationDateBytes.Length);
            offset += 8;

            Array.Copy(creatorCertificateDataLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(CreatorCertificateData, 0, data, offset, CreatorCertificateData.Length);
            offset += CreatorCertificateData.Length;

            Array.Copy(jobPayloadHashLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(JobPayloadHash, 0, data, offset, JobPayloadHash.Length);
            offset += JobPayloadHash.Length;

            Array.Copy(jobCreationSignatureDataLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(JobCreationSignatureData, 0, data, offset, JobCreationSignatureData.Length);
            offset += JobCreationSignatureData.Length;

            Array.Copy(jobDeletionSignatureDataLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(JobDeletionSignatureData, 0, data, offset, JobDeletionSignatureData.Length);
            offset += JobDeletionSignatureData.Length;

            Array.Copy(jobPayloadLength, 0, data, offset, 2);            
            offset += 2;
            Array.Copy(JobPayload, 0, data, offset, JobPayload.Length);

            return data;
        }

        /// <summary>
        /// Deserializes the job from a byte array
        /// </summary>
        /// <param name="data"></param>
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
            CreatorName = UTF8Encoding.UTF8.GetString(data, offset, creatorLength);
            offset += creatorLength;

            ushort numberOfBlocksLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            byte[] numberOfBlocks = new byte[numberOfBlocksLength];
            Array.Copy(data, offset, numberOfBlocks, 0, numberOfBlocksLength);
            NumberOfBlocks = new BigInteger(numberOfBlocks);
            offset += numberOfBlocksLength;

            CreationDate = DateTime.FromBinary(BitConverter.ToInt64(data, offset));
            offset += 8;

            ushort creatorCertificateDataLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            CreatorCertificateData = new byte[creatorCertificateDataLength];
            Array.Copy(data, offset, CreatorCertificateData, 0, creatorCertificateDataLength);
            offset += creatorCertificateDataLength;

            ushort jobPayloadHashLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobPayloadHash = new byte[jobPayloadHashLength];
            Array.Copy(data, offset, JobPayloadHash, 0, jobPayloadHashLength);
            offset += jobPayloadHashLength;

            ushort jobCreationSignatureDataLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobCreationSignatureData = new byte[jobCreationSignatureDataLength];
            Array.Copy(data, offset, JobCreationSignatureData, 0, jobCreationSignatureDataLength);
            offset += jobCreationSignatureDataLength;

            ushort jobDeleteionSignatureDataLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobDeletionSignatureData = new byte[jobDeleteionSignatureDataLength];
            Array.Copy(data, offset, JobDeletionSignatureData, 0, jobDeleteionSignatureDataLength);
            offset += jobDeleteionSignatureDataLength;

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
            builder.AppendLine("" + CreatorName + ",");
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

        /// <summary>
        /// Checks the creation signature
        /// </summary>
        /// <returns></returns>
        public bool HasValidCreationSignature()
        {
            try
            {
                X509Certificate2 creatorCertificate = new X509Certificate2(CreatorCertificateData);

                //some checks on the certificate
                if (!CertificateService.GetCertificateService().IsValidCertificate(creatorCertificate))
                {
                    return false;
                }
                if (CertificateService.GetCertificateService().IsBannedCertificate(creatorCertificate))
                {
                    return false;
                }     

                //1. Backup some fields and remove them, since they are not used in signature
                byte[] jobCreationSignatureDataBackup = JobCreationSignatureData;
                JobCreationSignatureData = new byte[0];
                byte[] jobDeletionSignatureDataBackup = JobDeletionSignatureData;
                JobDeletionSignatureData = new byte[0];
                byte[] payloadBackup = JobPayload;
                JobPayload = new byte[0];

                //2. Serialize for signature check
                byte[] data = Serialize();
                
                //3. Copy backups back
                JobCreationSignatureData = jobCreationSignatureDataBackup;
                JobDeletionSignatureData = jobDeletionSignatureDataBackup;
                JobPayload = payloadBackup;

                //5. Check signature; return false if not valid
                if (!CertificateService.GetCertificateService().VerifySignature(data,JobCreationSignatureData,creatorCertificate).Equals(CertificateValidationState.Valid))
                {
                    return false;
                }
                return true;                
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool HasValidDeletionSignature()
        {
            //todo: add code to check here...            
            return false;
        }

        public int CompareTo(Job other)
        {
            return -1 * CreationDate.CompareTo(other.CreationDate);
        }
    }
}
