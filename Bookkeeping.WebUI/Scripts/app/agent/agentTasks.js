$(function () {
    $(".task-list.mobile").on("click", ".mobile-task-item", function () {
        loading(true);
        window.location = $(this).data("location");
    })

    $(".title-tab").click(function () {
        $(".title-tab.active").removeClass("active");
        $(this).addClass("active");
        filter();
    })

    $(".no-new-tasks-btn").click(function () {
        filter();
    })

    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() == $(document).height()) {
            var displayedCount = $(".task-list .mobile-task-item").length;
            if (displayedCount >= pageSize) {
                filter(displayedCount);
            }
        }
    });
});

function filter(offset) {
    var offsetValue = offset ? offset : 0;
    var isDone = $(".title-tab.active").hasClass("is-done");
    var data = JSON.stringify({
        taskFilter: {
            Offset: offsetValue,
            Limit: pageSize,
            IsDone: isDone
        }
    });

    postAjax(
        getTasksUrl,
        data,
        function (resultData) {
            var taskList = $(".task-list");

            if (offsetValue > 0) {
                taskList.html(taskList.html() + resultData.tasksHtml);
            } else {
                taskList.html(resultData.tasksHtml);
            }

            if ($(".mobile-task-item").length == 0) {
                $(".no-tasks-wrap").show();
                if (isDone) {
                    $(".no-done-tasks").show();
                    $(".no-new-tasks").hide();
                } else {
                    $(".no-done-tasks").hide();
                    $(".no-new-tasks").show();
                }
            } else {
                $(".no-tasks-wrap").hide();
            }

            var doneCircle = $(".title-tab.is-done .numberCircle");
            var inWorkCircle = $(".title-tab.in-work .numberCircle");

            doneCircle.text(resultData.doneCount);
            if (resultData.doneCount > 0) {
                doneCircle.removeClass("empty");
            } else {
                doneCircle.addClass("empty");
            }

            inWorkCircle.text(resultData.inWorkCount);
            if (resultData.inWorkCount > 0) {
                inWorkCircle.removeClass("empty");
            } else {
                inWorkCircle.addClass("empty");
            }
        }
    );
}