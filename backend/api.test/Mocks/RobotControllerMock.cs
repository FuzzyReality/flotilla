﻿using Api.Controllers;
using Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
namespace Api.Test.Mocks
{
    internal class RobotControllerMock
    {
        public readonly Mock<IAreaService> AreaServiceMock;
        public readonly Mock<IIsarService> IsarServiceMock;
        public readonly Mock<IMissionRunService> MissionServiceMock;
        public readonly Mock<RobotController> Mock;
        public readonly Mock<IRobotModelService> RobotModelServiceMock;
        public readonly Mock<IRobotService> RobotServiceMock;

        public RobotControllerMock()
        {
            MissionServiceMock = new Mock<IMissionRunService>();
            IsarServiceMock = new Mock<IIsarService>();
            RobotServiceMock = new Mock<IRobotService>();
            RobotModelServiceMock = new Mock<IRobotModelService>();
            AreaServiceMock = new Mock<IAreaService>();

            var mockLoggerController = new Mock<ILogger<RobotController>>();

            Mock = new Mock<RobotController>(
                mockLoggerController.Object,
                RobotServiceMock.Object,
                IsarServiceMock.Object,
                MissionServiceMock.Object,
                RobotModelServiceMock.Object,
                AreaServiceMock.Object
            )
            {
                CallBase = true
            };
        }
    }
}
