﻿using System.Text.Json;
using Api.Controllers.Models;
using Api.Database.Models;
using Api.Services;
using Api.Services.Models;
using Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers
{
    [ApiController]
    [Route("robots")]
    public class RobotController(
            ILogger<RobotController> logger,
            IRobotService robotService,
            IIsarService isarService,
            IMissionRunService missionRunService,
            IRobotModelService robotModelService,
            IAreaService areaService
        ) : ControllerBase
    {
        /// <summary>
        ///     List all robots on the installation.
        /// </summary>
        /// <remarks>
        ///     <para> This query gets all robots </para>
        /// </remarks>
        [HttpGet]
        [Authorize(Roles = Role.Any)]
        [ProducesResponseType(typeof(IList<RobotResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IList<RobotResponse>>> GetRobots()
        {
            try
            {
                var robots = await robotService.ReadAll();
                var robotResponses = robots.Select(robot => new RobotResponse(robot));
                return Ok(robotResponses);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error during GET of robots  from database");
                throw;
            }
        }

        /// <summary>
        ///     Gets the robot with the specified id
        /// </summary>
        /// <remarks>
        ///     <para> This query gets the robot with the specified id </para>
        /// </remarks>
        [HttpGet]
        [Authorize(Roles = Role.Any)]
        [Route("{id}")]
        [ProducesResponseType(typeof(RobotResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RobotResponse>> GetRobotById([FromRoute] string id)
        {
            logger.LogInformation("Getting robot with id={Id}", id);
            try
            {
                var robot = await robotService.ReadById(id);
                if (robot == null)
                {
                    logger.LogWarning("Could not find robot with id={Id}", id);
                    return NotFound();
                }

                var robotResponse = new RobotResponse(robot);
                logger.LogInformation("Successful GET of robot with id={id}", id);
                return Ok(robotResponse);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error during GET of robot with id={Id}", id);
                throw;
            }
        }

        /// <summary>
        ///     Create robot and add to database
        /// </summary>
        /// <remarks>
        ///     <para> This query creates a robot and adds it to the database </para>
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        [ProducesResponseType(typeof(RobotResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RobotResponse>> CreateRobot([FromBody] CreateRobotQuery robotQuery)
        {
            logger.LogInformation("Creating new robot");
            try
            {
                var robotModel = await robotModelService.ReadByRobotType(robotQuery.RobotType);
                if (robotModel == null)
                {
                    return BadRequest(
                        $"No robot model exists with robot type '{robotQuery.RobotType}'"
                    );
                }

                var robot = new Robot(robotQuery)
                {
                    Model = robotModel
                };

                var newRobot = await robotService.Create(robot);
                var robotResponses = new RobotResponse(newRobot);

                logger.LogInformation("Succesfully created new robot");
                return CreatedAtAction(nameof(GetRobotById), new
                {
                    id = newRobot.Id
                }, robotResponses);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while creating new robot");
                throw;
            }
        }

        /// <summary>
        ///     Updates a robot in the database
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200"> The robot was successfully updated </response>
        /// <response code="400"> The robot data is invalid </response>
        /// <response code="404"> There was no robot with the given ID in the database </response>
        [HttpPut]
        [Authorize(Roles = Role.Admin)]
        [Route("{id}")]
        [ProducesResponseType(typeof(RobotResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RobotResponse>> UpdateRobot(
            [FromRoute] string id,
            [FromBody] Robot robot
        )
        {
            logger.LogInformation("Updating robot with id={Id}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data");
            }

            if (id != robot.Id)
            {
                logger.LogWarning("Id: {Id} not corresponding to updated robot", id);
                return BadRequest("Inconsistent Id");
            }

            try
            {
                var updatedRobot = await robotService.Update(robot);
                var robotResponse = new RobotResponse(updatedRobot);
                logger.LogInformation("Successful PUT of robot to database");

                return Ok(robotResponse);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while updating robot with id={Id}", id);
                throw;
            }
        }

        /// <summary>
        ///     Deletes the robot with the specified id from the database
        /// </summary>
        [HttpDelete]
        [Authorize(Roles = Role.Admin)]
        [Route("{id}")]
        [ProducesResponseType(typeof(MissionRun), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RobotResponse>> DeleteRobot([FromRoute] string id)
        {
            var robot = await robotService.Delete(id);
            if (robot is null)
            {
                return NotFound($"Robot with id {id} not found");
            }
            var robotResponse = new RobotResponse(robot);
            return Ok(robotResponse);
        }

        /// <summary>
        ///     Updates a robot's status in the database
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200"> The robot status was successfully updated </response>
        /// <response code="400"> The robot data is invalid </response>
        /// <response code="404"> There was no robot with the given ID in the database </response>
        [HttpPut]
        [Authorize(Roles = Role.Admin)]
        [Route("{id}/status")]
        [ProducesResponseType(typeof(RobotResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RobotResponse>> UpdateRobotStatus(
            [FromRoute] string id,
            [FromBody] RobotStatus robotStatus
        )
        {
            logger.LogInformation("Updating robot status with id={Id}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data");
            }

            var robot = await robotService.ReadById(id);
            if (robot == null)
            {
                string errorMessage = $"No robot with id: {id} could be found";
                logger.LogError("{Message}", errorMessage);
                return NotFound(errorMessage);
            }

            robot.Status = robotStatus;

            try
            {
                var updatedRobot = await robotService.Update(robot);
                var robotResponse = new RobotResponse(updatedRobot);

                logger.LogInformation("Successful PUT of robot to database");

                return Ok(robotResponse);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while updating status for robot with id={Id}", id);
                throw;
            }
        }

        /// <summary>
        ///     Get video streams for a given robot
        /// </summary>
        /// <remarks>
        ///     <para> Retrieves the video streams available for the given robot </para>
        /// </remarks>
        [HttpGet]
        [Authorize(Roles = Role.User)]
        [Route("{robotId}/video-streams/")]
        [ProducesResponseType(typeof(IList<VideoStream>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IList<VideoStream>>> GetVideoStreams([FromRoute] string robotId)
        {
            var robot = await robotService.ReadById(robotId);
            if (robot == null)
            {
                logger.LogWarning("Could not find robot with id={Id}", robotId);
                return NotFound();
            }

            return Ok(robot.VideoStreams);
        }

        /// <summary>
        ///     Add a video stream to a given robot
        /// </summary>
        /// <remarks>
        ///     <para> Adds a provided video stream to the given robot </para>
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        [Route("{robotId}/video-streams/")]
        [ProducesResponseType(typeof(RobotResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RobotResponse>> CreateVideoStream(
            [FromRoute] string robotId,
            [FromBody] VideoStream videoStream
        )
        {
            var robot = await robotService.ReadById(robotId);
            if (robot == null)
            {
                logger.LogWarning("Could not find robot with id={Id}", robotId);
                return NotFound();
            }

            robot.VideoStreams.Add(videoStream);

            try
            {
                var updatedRobot = await robotService.Update(robot);
                var robotResponse = new RobotResponse(updatedRobot);

                return CreatedAtAction(
                    nameof(GetVideoStreams),
                    new
                    {
                        robotId = updatedRobot.Id
                    },
                    robotResponse
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding video stream to robot");
                throw;
            }
        }

        /// <summary>
        ///     Start the mission in the database with the corresponding 'missionRunId' for the robot with id 'robotId'
        /// </summary>
        /// <remarks>
        ///     <para> This query starts a mission for a given robot </para>
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        [Route("{robotId}/start/{missionRunId}")]
        [ProducesResponseType(typeof(MissionRun), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MissionRun>> StartMission(
            [FromRoute] string robotId,
            [FromRoute] string missionRunId
        )
        {
            var robot = await robotService.ReadById(robotId);

            if (robot == null)
            {
                logger.LogWarning("Could not find robot with id={Id}", robotId);
                return NotFound("Robot not found");
            }

            if (robot.Status is not RobotStatus.Available)
            {
                logger.LogWarning(
                    "Robot '{Id}' is not available ({Status})",
                    robotId,
                    robot.Status.ToString()
                );
                return Conflict($"The Robot is not available ({robot.Status})");
            }

            var missionRun = await missionRunService.ReadById(missionRunId);

            if (missionRun == null)
            {
                logger.LogWarning("Could not find mission with id={Id}", missionRunId);
                return NotFound("Mission not found");
            }

            IsarMission isarMission;
            try
            {
                isarMission = await isarService.StartMission(robot, missionRun);
            }
            catch (HttpRequestException e)
            {
                string errorMessage = $"Could not reach ISAR at {robot.IsarUri}";
                logger.LogError(e, "{Message}", errorMessage);
                await OnIsarUnavailable(robot);
                return StatusCode(StatusCodes.Status502BadGateway, errorMessage);
            }
            catch (MissionException e)
            {
                logger.LogError(e, "Error while starting ISAR mission");
                return StatusCode(StatusCodes.Status502BadGateway, $"{e.Message}");
            }
            catch (JsonException e)
            {
                const string Message = "Error while processing of the response from ISAR";
                logger.LogError(e, "{Message}", Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Message);
            }
            catch (RobotPositionNotFoundException e)
            {
                const string Message = "A suitable robot position could not be found for one or more of the desired tags";
                logger.LogError(e, "{Message}", Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Message);
            }

            missionRun.UpdateWithIsarInfo(isarMission);
            missionRun.Status = MissionStatus.Ongoing;

            await missionRunService.Update(missionRun);

            robot.Status = RobotStatus.Busy;

            try
            {
                await robotService.UpdateRobotStatus(robot.Id, RobotStatus.Busy);
                await robotService.UpdateCurrentMissionId(robot.Id, missionRun.Id);
            }
            catch (RobotNotFoundException e) { return NotFound(e.Message); }

            return Ok(missionRun);
        }

        /// <summary>
        ///     Stops the current mission on a robot
        /// </summary>
        /// <remarks>
        ///     <para> This query stops the current mission for a given robot </para>
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = Role.User)]
        [Route("{robotId}/stop/")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> StopMission([FromRoute] string robotId)
        {
            var robot = await robotService.ReadById(robotId);
            if (robot == null)
            {
                logger.LogWarning("Could not find robot with id={Id}", robotId);
                return NotFound();
            }

            try { await isarService.StopMission(robot); }
            catch (HttpRequestException e)
            {
                const string Message = "Error connecting to ISAR while stopping mission";
                logger.LogError(e, "{Message}", Message);
                await OnIsarUnavailable(robot);
                return StatusCode(StatusCodes.Status502BadGateway, Message);
            }
            catch (MissionException e)
            {
                logger.LogError(e, "Error while stopping ISAR mission");
                return StatusCode(StatusCodes.Status502BadGateway, $"{e.Message}");
            }
            catch (JsonException e)
            {
                const string Message = "Error while processing the response from ISAR";
                logger.LogError(e, "{Message}", Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Message);
            }
            catch (MissionNotFoundException)
            {
                logger.LogWarning($"No mission was runnning for robot {robot.Id}");
                try { await robotService.UpdateCurrentMissionId(robotId, null); }
                catch (RobotNotFoundException e) { return NotFound(e.Message); }

            }
            try { await robotService.UpdateCurrentMissionId(robotId, null); }
            catch (RobotNotFoundException e) { return NotFound(e.Message); }

            return NoContent();
        }

        /// <summary>
        ///     Pause the current mission on a robot
        /// </summary>
        /// <remarks>
        ///     <para> This query pauses the current mission for a robot </para>
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = Role.User)]
        [Route("{robotId}/pause/")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PauseMission([FromRoute] string robotId)
        {
            var robot = await robotService.ReadById(robotId);
            if (robot == null)
            {
                logger.LogWarning("Could not find robot with id={Id}", robotId);
                return NotFound();
            }

            try
            {
                await isarService.PauseMission(robot);
            }
            catch (HttpRequestException e)
            {
                const string Message = "Error connecting to ISAR while pausing mission";
                logger.LogError(e, "{Message}", Message);
                await OnIsarUnavailable(robot);
                return StatusCode(StatusCodes.Status502BadGateway, Message);
            }
            catch (MissionException e)
            {
                logger.LogError(e, "Error while pausing ISAR mission");
                return StatusCode(StatusCodes.Status502BadGateway, $"{e.Message}");
            }
            catch (JsonException e)
            {
                const string Message = "Error while processing of the response from ISAR";
                logger.LogError(e, "{Message}", Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Message);
            }

            return NoContent();
        }

        /// <summary>
        ///     Resume paused mission on a robot
        /// </summary>
        /// <remarks>
        ///     <para> This query resumes the currently paused mission for a robot </para>
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = Role.User)]
        [Route("{robotId}/resume/")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ResumeMission([FromRoute] string robotId)
        {
            var robot = await robotService.ReadById(robotId);
            if (robot == null)
            {
                logger.LogWarning("Could not find robot with id={Id}", robotId);
                return NotFound();
            }

            try
            {
                await isarService.ResumeMission(robot);
            }
            catch (HttpRequestException e)
            {
                const string Message = "Error connecting to ISAR while resuming mission";
                logger.LogError(e, "{Message}", Message);
                await OnIsarUnavailable(robot);
                return StatusCode(StatusCodes.Status502BadGateway, Message);
            }
            catch (MissionException e)
            {
                logger.LogError(e, "Error while resuming ISAR mission");
                return StatusCode(StatusCodes.Status502BadGateway, $"{e.Message}");
            }
            catch (JsonException e)
            {
                const string Message = "Error while processing of the response from ISAR";
                logger.LogError(e, "{Message}", Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Message);
            }

            return NoContent();
        }


        /// <summary>
        ///     Post new arm position ("battery_change", "transport", "lookout") for the robot with id 'robotId'
        /// </summary>
        /// <remarks>
        ///     <para> This query moves the arm to a given position for a given robot </para>
        /// </remarks>
        [HttpPut]
        [Authorize(Roles = Role.User)]
        [Route("{robotId}/SetArmPosition/{armPosition}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SetArmPosition(
            [FromRoute] string robotId,
            [FromRoute] string armPosition
        )
        {
            var robot = await robotService.ReadById(robotId);
            if (robot == null)
            {
                string errorMessage = $"Could not find robot with id {robotId}";
                logger.LogWarning("{Message}", errorMessage);
                return NotFound(errorMessage);
            }

            if (robot.Status is not RobotStatus.Available)
            {
                string errorMessage = $"Robot {robotId} has status ({robot.Status}) and is not available";
                logger.LogWarning("{Message}", errorMessage);
                return Conflict(errorMessage);
            }
            try { await isarService.StartMoveArm(robot, armPosition); }
            catch (HttpRequestException e)
            {
                string errorMessage = $"Error connecting to ISAR at {robot.IsarUri}";
                logger.LogError(e, "{Message}", errorMessage);
                await OnIsarUnavailable(robot);
                return StatusCode(StatusCodes.Status502BadGateway, errorMessage);
            }
            catch (MissionException e)
            {
                const string ErrorMessage = "An error occurred while setting the arm position mission";
                logger.LogError(e, "{Message}", ErrorMessage);
                return StatusCode(StatusCodes.Status502BadGateway, ErrorMessage);
            }
            catch (JsonException e)
            {
                const string ErrorMessage = "Error while processing of the response from ISAR";
                logger.LogError(e, "{Message}", ErrorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessage);
            }

            return NoContent();
        }

        /// <summary>
        ///     Start a localization mission with localization in the pose 'localizationPose' for the robot with id 'robotId'
        /// </summary>
        /// <remarks>
        ///     <para> This query starts a localization for a given robot </para>
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = Role.User)]
        [Route("start-localization")]
        [ProducesResponseType(typeof(MissionRun), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MissionRun>> StartLocalizationMission(
            [FromBody] ScheduleLocalizationMissionQuery scheduleLocalizationMissionQuery
        )
        {
            var robot = await robotService.ReadById(scheduleLocalizationMissionQuery.RobotId);
            if (robot == null)
            {
                logger.LogWarning("Could not find robot with id={Id}", scheduleLocalizationMissionQuery.RobotId);
                return NotFound("Robot not found");
            }

            if (robot.Status is not RobotStatus.Available)
            {
                logger.LogWarning(
                    "Robot '{Id}' is not available ({Status})",
                    scheduleLocalizationMissionQuery.RobotId,
                    robot.Status.ToString()
                );
                return Conflict($"The Robot is not available ({robot.Status})");
            }

            var area = await areaService.ReadById(scheduleLocalizationMissionQuery.AreaId);

            if (area == null)
            {
                logger.LogWarning("Could not find area with id={Id}", scheduleLocalizationMissionQuery.AreaId);
                return NotFound("Area not found");
            }

            var missionRun = new MissionRun
            {
                Name = "Localization Mission",
                Robot = robot,
                MissionRunPriority = MissionRunPriority.Normal,
                InstallationCode = "NA",
                Area = area,
                Status = MissionStatus.Pending,
                DesiredStartTime = DateTime.UtcNow,
                Tasks = new List<MissionTask>(),
                Map = new MapMetadata()
            };

            IsarMission isarMission;
            try
            {
                isarMission = await isarService.StartLocalizationMission(robot, scheduleLocalizationMissionQuery.LocalizationPose);
            }
            catch (HttpRequestException e)
            {
                string message = $"Could not reach ISAR at {robot.IsarUri}";
                logger.LogError(e, "{Message}", message);
                await OnIsarUnavailable(robot);
                return StatusCode(StatusCodes.Status502BadGateway, message);
            }
            catch (MissionException e)
            {
                logger.LogError(e, "Error while starting ISAR localization mission");
                return StatusCode(StatusCodes.Status502BadGateway, $"{e.Message}");
            }
            catch (JsonException e)
            {
                const string Message = "Error while processing of the response from ISAR";
                logger.LogError(e, "{Message}", Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Message);
            }

            missionRun.UpdateWithIsarInfo(isarMission);
            missionRun.Status = MissionStatus.Ongoing;

            await missionRunService.Create(missionRun);

            try
            {
                await robotService.UpdateRobotStatus(robot.Id, RobotStatus.Busy);
                await robotService.UpdateCurrentMissionId(robot.Id, missionRun.Id);
                await robotService.UpdateCurrentArea(robot.Id, area);
            }
            catch (RobotNotFoundException e) { return NotFound(e.Message); }

            return Ok(missionRun);
        }

        private async Task OnIsarUnavailable(Robot robot)
        {
            robot.Enabled = false;
            robot.Status = RobotStatus.Offline;
            if (robot.CurrentMissionId != null)
            {
                var missionRun = await missionRunService.ReadById(robot.CurrentMissionId);
                if (missionRun != null)
                {
                    missionRun.SetToFailed();
                    await missionRunService.Update(missionRun);
                    logger.LogWarning(
                        "Mission '{Id}' failed because ISAR could not be reached",
                        missionRun.Id
                    );
                }
            }

            await robotService.UpdateCurrentMissionId(robot.Id, null);
        }
    }
}
