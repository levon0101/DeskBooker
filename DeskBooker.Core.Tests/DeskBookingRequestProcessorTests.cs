using System;
using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
using Xunit;

namespace DeskBooker.Core
{
    public class DeskBookingRequestProcessorTests
    {
        private DeskBookingRequestProcessor _processor;
        public DeskBookingRequestProcessorTests()
        {
            _processor = new DeskBookingRequestProcessor();

        }

        [Fact]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {
            //Arrange
            var request = new DeskBookingRequest
            {
                FirstName = "Thomas",
                LastName = "Huber",
                Email = "thomas@cloudiushuber@gmail.com",
                Date = new DateTime(2012,12,1)
            };
             
            //Act
            DeskBookingResult result = _processor.BookDesk(request);

            //Assert

            Assert.NotNull(result);

            Assert.Equal(request.FirstName, result.FirstName);
            Assert.Equal(request.LastName, result.LastName);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal(request.Date, result.Date);

        }

        [Fact]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            //Arrange
             
            Assert.Throws<ArgumentNullException>(() => { _processor.BookDesk(null); });

        }
    }
}
