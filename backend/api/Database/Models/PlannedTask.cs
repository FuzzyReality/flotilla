﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Controllers.Models;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Api.Database.Models
{
    [Owned]
    public class PlannedTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public int PlanOrder { get; set; }

        [MaxLength(200)]
        public string TagId { get; set; }

        [MaxLength(200)]
        public Uri URL { get; set; }

        [Required]
        public Position TagPosition { get; set; }

        [Required]
        public Pose Pose { get; set; }
        public int PoseId { get; set; }

        public IList<PlannedInspection> Inspections { get; set; }

        public PlannedTask()
        {
            Inspections = new List<PlannedInspection>();
        }

        public PlannedTask(EchoTag echoTag, Position tagPosition)
        {
            Inspections = echoTag.Inspections
                .Select(inspection => new PlannedInspection(inspection))
                .ToList();
            URL = echoTag.URL;
            TagId = echoTag.TagId;
            TagPosition = tagPosition;
            Pose = echoTag.Pose;
            PoseId = echoTag.PoseId;
            PlanOrder = echoTag.PlanOrder;
        }
    }
}
