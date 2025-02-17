﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618
namespace Api.Database.Models
{
    public class Area
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public virtual Deck? Deck { get; set; }

        public virtual Plant Plant { get; set; }

        public virtual Installation Installation { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        public MapMetadata MapMetadata { get; set; }


        public DefaultLocalizationPose? DefaultLocalizationPose { get; set; }

        public IList<SafePosition> SafePositions { get; set; }
    }

    public class SafePosition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public Pose Pose { get; set; }

        public SafePosition()
        {
            Pose = new Pose();
        }

        public SafePosition(Pose pose)
        {
            Pose = pose;
        }
    }
}
