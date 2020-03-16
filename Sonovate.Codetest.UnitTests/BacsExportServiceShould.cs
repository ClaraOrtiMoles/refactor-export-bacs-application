namespace Sonovate.Codetest.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using AutoFixture;
	using CodeTest;
	using CodeTest.Configuration;
	using CodeTest.Domain;
	using FluentAssertions;
	using Moq;
	using NUnit.Framework;

	public class BacsExportServiceShould
	{
		private readonly BacsExportService _bacsExportService = new BacsExportService();

		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void ThrowException_WhenExportingZip_GivenBacsExportTypeIsNone()
		{
			Func<Task> action = () =>_bacsExportService.ExportZip(BacsExportType.None);
			action.Should().ThrowAsync<Exception>().WithMessage("No export type provided.");
		}

		[Test]
		public void NotThrowException_WhenExportingZip_GivenBacsExportTypeIsAgencyAndNotEnabledAgencyPayments()
		{
			var settingsMock = new Mock<ISettings>();
			settingsMock.Setup(x => x.GetSetting("EnableAgencyPayments")).Returns("false");

			_bacsExportService.SetSettings(settingsMock.Object);
			Func<Task> action = () => _bacsExportService.ExportZip(BacsExportType.Agency);
			action.Should().NotThrowAsync<Exception>();
		}

		[Test]
		public void ThrowException_WhenExportingZip_GivenBacsExportTypeIsAgencyAndGetAgencyBacsResultThrowsInvalidOperationException()
		{
			var settingsMock = new Mock<ISettings>();
			settingsMock.Setup(x => x.GetSetting("EnableAgencyPayments")).Returns("false");
			var agencyPaymentMock = new Mock<IAgencyPaymentService>();
			var exceptionMessage = "blah";
			agencyPaymentMock.Setup(x => x.GetAgencyBacsResult(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.ThrowsAsync(new InvalidOperationException(exceptionMessage));
			_bacsExportService.SetSettings(settingsMock.Object);
			_bacsExportService.SetAgencyPaymentService(agencyPaymentMock.Object);

			Func<Task> action = () => _bacsExportService.ExportZip(BacsExportType.Agency);
			action.Should().ThrowAsync<Exception>().WithMessage(exceptionMessage);
		}

		[Test]
		public async Task ExportAgencyBacsResults_WhenExportingZip_GivenBacsExportTypeIsAgencyAndNotEnabledAgencyPayments()
		{
			var settingsMock = new Mock<ISettings>();
			settingsMock.Setup(x => x.GetSetting("EnableAgencyPayments")).Returns("true");
			var agencyPaymentMock = new Mock<IAgencyPaymentService>();
			var bacsResults = (new Fixture()).Create<List<BacsResult>>();
			agencyPaymentMock.Setup(x => x.GetAgencyBacsResult(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.ReturnsAsync(bacsResults);
			var csvFileWriterMock = new Mock<ICsvFileWriter>();
			_bacsExportService.SetSettings(settingsMock.Object);
			_bacsExportService.SetAgencyPaymentService(agencyPaymentMock.Object);
			_bacsExportService.SetCsvFileWriter(csvFileWriterMock.Object);

			await _bacsExportService.ExportZip(BacsExportType.Agency);

			csvFileWriterMock.Verify(x => x.WriteCsvFile($"{BacsExportType.Agency.ToString()}_BACSExport.csv", bacsResults), Times.Once);
		}
		 
	}
}