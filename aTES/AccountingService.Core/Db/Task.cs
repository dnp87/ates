using Common.Enums;
using LinqToDB.Mapping;
using System;

namespace AccountingService.Core.Db
{
    [Table(Name = "TASKS")]
    public class Task
    {
        [PrimaryKey]
        [Identity]
        [Column(Name = "ID")]
        public int Id { get; set; }

        // should be guid, but I failed to make it work with linq2db yet
        [Column(Name = "PUBLIC_ID")]
        public string PublicId { get; set; }

        [Column(Name = "NAME")]
        public string Name { get; set; }

        [Column(Name = "JIRA_ID")]
        public string JiraId { get; set; }

        [Column(Name = "DESCRIPTION")]
        public string Description { get; set; }

        [Column(Name = "STATUS_ID")]
        public TaskStatus Status { get; set; }

        [Column(Name = "PARROT_ID")]
        public int ParrotId { get; set; }

        [Association(ThisKey = nameof(ParrotId), OtherKey = nameof(Db.Parrot.Id))]
        public Parrot Parrot { get; set; }

        [Column(Name = "ASSIGNED_AMOUNT")]
        public int AssignedAmount { get; set; }

        [Column(Name = "COMPLETED_AMOUNT")]
        public int CompletedAmount { get; set; }

        [Column(Name = "DATE_COMPLETED")]
        public DateTime DateCompleted { get; set; }
    }
}