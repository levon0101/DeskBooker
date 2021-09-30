using System;
using System.Collections.Generic;
using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit;

namespace DeskBooker.Web.Pages
{
    public class BookDeskModelTests
    {
        private BookDeskModel _bookDeskModel;
        private Mock<IDeskBookingRequestProcessor> _processorMock;
        private DeskBookingResult _deskBookingResult;

        public BookDeskModelTests()
        {

            _processorMock = new Mock<IDeskBookingRequestProcessor>();

            _bookDeskModel = new BookDeskModel(_processorMock.Object)
            {
                DeskBookingRequest = new DeskBookingRequest()
            };

            _deskBookingResult = new DeskBookingResult
            {
                Code = DeskBookingResultCode.Success
            };

            _processorMock.Setup(x => x.BookDesk(_bookDeskModel.DeskBookingRequest))
                .Returns(_deskBookingResult);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public void ShouldCallBookDeskMethodOfProcessorIfModelIsValid(int expectedBookDeskCalls, bool isModelValid)
        {

            if (!isModelValid)
            {
                _bookDeskModel.ModelState.AddModelError("some key", "some error");
            }

            _bookDeskModel.OnPost();

            _processorMock.Verify(x => x.BookDesk(_bookDeskModel.DeskBookingRequest), Times.Exactly(expectedBookDeskCalls));

        }


        [Fact]
        public void ShouldAddModelErrorIfNoDeskIsAvailable()
        {

            _deskBookingResult.Code = DeskBookingResultCode.NoDeskAvailable;

            _bookDeskModel.OnPost();

            var modelStateEntry = Assert.Contains("DeskBookingRequest.Date", _bookDeskModel.ModelState);

            var modelError = Assert.Single(modelStateEntry.Errors);

            Assert.Equal("No desk available for selected date.", modelError.ErrorMessage);
        }


        [Fact]
        public void ShouldNotAddModelErrorIfDeskIsAvailable()
        {
            _deskBookingResult.Code = DeskBookingResultCode.Success;

            _bookDeskModel.OnPost();

            Assert.DoesNotContain("DeskBookingRequest.Date", _bookDeskModel.ModelState);
        }

        [Theory]
        [InlineData(typeof(PageResult), false, null)]
        [InlineData(typeof(PageResult), true, DeskBookingResultCode.NoDeskAvailable)]
        [InlineData(typeof(RedirectToPageResult), true, DeskBookingResultCode.Success)]
        public void ShouldReturnExpectedActionResult(Type expectedResultType, bool isModelStateValid, DeskBookingResultCode? deskBookingResultCode)
        {
            if (!isModelStateValid)
            {
                _bookDeskModel.ModelState.AddModelError("Just a key", "An error message");
            }

            if (deskBookingResultCode.HasValue)
            {
                _deskBookingResult.Code = deskBookingResultCode.Value;
            }

            var actionResult = _bookDeskModel.OnPost();

            Assert.IsType(expectedResultType, actionResult);
        }

        [Fact]
        public void ShouldRedirectToBookDeskConfirmationPage()
        {
            _deskBookingResult.Code = DeskBookingResultCode.Success;
            _deskBookingResult.DeskBookingId = 7;
            _deskBookingResult.FirstName = "Levon";
            _deskBookingResult.Date = new DateTime(2022, 01, 1);

            IActionResult actionResult = _bookDeskModel.OnPost();
            var redirectToPageResult = Assert.IsType<RedirectToPageResult>(actionResult);

            Assert.Equal("BookDeskConfirmation", redirectToPageResult.PageName);

            var routValues = redirectToPageResult.RouteValues;

            Assert.Equal(3, routValues.Count);

            var deskBookingId = Assert.Contains("DeskBookingId", (IReadOnlyDictionary<string, object>)routValues);
            Assert.Equal(_deskBookingResult.DeskBookingId, deskBookingId);

            var firstName = Assert.Contains("DeskBookingId", (IReadOnlyDictionary<string, object>)routValues);
            Assert.Equal(_deskBookingResult.FirstName, firstName);

            var date = Assert.Contains("DeskBookingId", (IReadOnlyDictionary<string, object>)routValues);
            Assert.Equal(_deskBookingResult.Date, date);

        }

    }
}
