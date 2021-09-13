using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
using Moq;
using Xunit;

namespace DeskBooker.Core
{
    public class DeskBookingRequestProcessorTests
    {
        private DeskBookingRequestProcessor _processor;
        private DeskBookingRequest _request;
        private Mock<IDeskBookingRepository> _deskBookingRepository;
        private Mock<IDeskRepository> _deskRepository;
        private List<Desk> _availableDesks;

        public DeskBookingRequestProcessorTests()
        {
            _request = new DeskBookingRequest
            {
                FirstName = "Thomas",
                LastName = "Huber",
                Email = "thomas@cloudiushuber@gmail.com",
                Date = new DateTime(2012, 12, 1)
            };

            _availableDesks = new List<Desk>
             {
                 new Desk{Id = 7}
             };

            _deskBookingRepository = new Mock<IDeskBookingRepository>();
            _deskRepository = new Mock<IDeskRepository>();

            _deskRepository.Setup(x => x.GetAvailableDesks(_request.Date)).Returns(_availableDesks);


            _processor = new DeskBookingRequestProcessor(_deskBookingRepository.Object, _deskRepository.Object);

        }

        [Fact]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {


            //Act
            DeskBookingResult result = _processor.BookDesk(_request);

            //Assert

            Assert.NotNull(result);

            Assert.Equal(_request.FirstName, result.FirstName);
            Assert.Equal(_request.LastName, result.LastName);
            Assert.Equal(_request.Email, result.Email);
            Assert.Equal(_request.Date, result.Date);

        }

        [Fact]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            //Arrange

            Assert.Throws<ArgumentNullException>(() => { _processor.BookDesk(null); });

        }

        [Fact]
        public void ShouldSaveDeskBooking()
        {
            DeskBooking savedDeskBooking = null;

            _deskBookingRepository.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                .Callback<DeskBooking>(deskBooking => { savedDeskBooking = deskBooking; });

            _processor.BookDesk(_request);

            _deskBookingRepository.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Once);

            Assert.NotNull(savedDeskBooking);

            Assert.Equal(_request.FirstName, savedDeskBooking.FirstName);
            Assert.Equal(_request.LastName, savedDeskBooking.LastName);
            Assert.Equal(_request.Email, savedDeskBooking.Email);
            Assert.Equal(_request.Date, savedDeskBooking.Date);
            Assert.Equal(_availableDesks.First().Id, savedDeskBooking.DeskId);
        }


        [Fact]
        public void ShouldNotSaveDeskBookingIfNoDeskIsAvailable()
        {
            _availableDesks.Clear();

            _processor.BookDesk(_request);

            _deskBookingRepository.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Never);

        }


        [Theory]
        [InlineData(DeskBookingResultCode.Success, true)]
        [InlineData(DeskBookingResultCode.NoDeskAvailable, false)]
        public void ShouldReturnExpectedResultCode(DeskBookingResultCode expectedResultCode, bool iSDeskAvailable)
        {
            if (!iSDeskAvailable)
                _availableDesks.Clear();

            var result = _processor.BookDesk(_request);

            Assert.Equal(expectedResultCode, result.Code);
        }

        [Theory]
        [InlineData(7, true)]
        [InlineData(null, false)]
        public void ShouldReturnExpectedDeskBookingId(int? expectedDeskBookingId, bool iSDeskAvailable)
        {
            if (!iSDeskAvailable)
            {
                _availableDesks.Clear();
            }
            else
            {
                _deskBookingRepository.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                    .Callback<DeskBooking>(deskBooking =>
                    {
                        deskBooking.Id = expectedDeskBookingId.Value;

                    });
            }

            var result = _processor.BookDesk(_request);


            Assert.Equal(expectedDeskBookingId, result.DeskBookingId);
        }
    }
}
