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
		private readonly Fixture _fixture = new Fixture();
		private readonly BacsExportService _bacsExportService = new BacsExportService();
		private Mock<ISettings> _settingsMock;
		private Mock<IAgencyPaymentService> _agencyPaymentMock;
		private Mock<ICsvFileWriter> _csvFileWriterMock;
		private string _exceptionMessage;
		
		[SetUp]
		public void Setup()
		{
			_settingsMock = new Mock<ISettings>();
			_agencyPaymentMock = new Mock<IAgencyPaymentService>();
			_csvFileWriterMock = new Mock<ICsvFileWriter>();

			_bacsExportService.SetSettings(_settingsMock.Object);
			_bacsExportService.SetAgencyPaymentService(_agencyPaymentMock.Object);
			_bacsExportService.SetCsvFileWriter(_csvFileWriterMock.Object);

			_exceptionMessage = _fixture.Create<string>();
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
			_settingsMock.Setup(x => x.GetSetting("EnableAgencyPayments")).Returns("false");

			Func<Task> action = () => _bacsExportService.ExportZip(BacsExportType.Agency);
			action.Should().NotThrowAsync<Exception>();
		}

		[Test]
		public void ThrowException_WhenExportingZip_GivenBacsExportTypeIsAgencyAndGetAgencyBacsResultThrowsInvalidOperationException()
		{
			_settingsMock.Setup(x => x.GetSetting("EnableAgencyPayments")).Returns("false");
			_agencyPaymentMock.Setup(x => x.GetAgencyBacsResult(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.ThrowsAsync(new InvalidOperationException(_exceptionMessage));

			Func<Task> action = () => _bacsExportService.ExportZip(BacsExportType.Agency);
			action.Should().ThrowAsync<Exception>().WithMessage(_exceptionMessage);
		}

		[Test]
		public async Task ExportAgencyBacsResults_WhenExportingZip_GivenBacsExportTypeIsAgencyAndNotEnabledAgencyPayments()
		{
			_settingsMock.Setup(x => x.GetSetting("EnableAgencyPayments")).Returns("true");
			var bacsResults = _fixture.Create<List<BacsResult>>();
			_agencyPaymentMock.Setup(x => x.GetAgencyBacsResult(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.ReturnsAsync(bacsResults);

			await _bacsExportService.ExportZip(BacsExportType.Agency);

			_csvFileWriterMock.Verify(x => x.WriteCsvFile($"{BacsExportType.Agency.ToString()}_BACSExport.csv", bacsResults), Times.Once);
		}
		 
	}
}