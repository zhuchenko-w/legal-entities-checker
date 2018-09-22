var lastSearchedInn;
var lastFoundNameByInn;

$(function () {
    bindContextMenu();

    $(".task-list").on("click", ".task-item", function () {
        openTaskCard($(this));
    });
    $(".close-task-card-icon").click(function () {
        closeTaskCard();
        return false;
    });

    $(".open-menu-icon").click(function () {
        openNav();
    });
    $(document).mouseup(function (e) {
        var container = $("#mainSidenav");
        if (!container.is(e.target) && container.has(e.target).length === 0) {
            closeNav();
        }
    });

    $(".task-list").on("mouseenter", ".task-item .result .user-icon", function (e) {
        var selector = "#progress-tooltip";
        var tooltip = $(selector);

        var taskId = $(".id", $(this).closest(".task-item")).val();

        if (tooltip.data("task-id") !== taskId) {
            tooltip.data("task-id", taskId);

            var decisionsJson = $(this).data("decisions-json");
            var tooltipHtml = "";

            $.each(decisionsJson, function (index, decision) {
                tooltipHtml +=
                    "<li>" + 
                        "<div class='task-tooltip-item'>" +
                            "<span class='task-tooltip-item-agent'>" + decision.AgentCode + "</span>" + 
                            (decision.Decision === null ? "" : "<div class='task-tooltip-item-icon task-tooltip-item-icon-" + (decision.Decision === true ? "ok" : "alert") + "'></div>") + 
                        "</div>" +
                    "</li>";
            });

            tooltip.html(tooltipHtml);
        }

        tooltip.css({
            left: getPosition(selector, e.clientX + 1, "outerWidth", "scrollLeft"),
            top: getPosition(selector, e.clientY + 1 , "outerHeight", "scrollTop")
        });
        tooltip.show();
    }).on("mouseleave", ".task-item, .result .user-icon", function (e) {
        $("#progress-tooltip").hide();
    });

    $(".btn-add-task").click(function () {
        openNewTaskDialog();
    });
    $(".close-new-task-dialog-icon").click(function () {
        closeNewTaskDialog();
    });

    $("#new-task-card-agents").parent(".dropdown").on("hide.bs.dropdown", function () {
        validateNewTask();
    });

    $(".btn-create-new-task").click(function () {
        createNewTask();
    });

    $(".task-card").on("click", ".download-resolution-image-icon", function () {
        downloadImage($("img.image", $(this).closest(".resolution-image")));
    });

    $("#inn, #name, #purpose").keypress(enterKeyFilterHandler);
    $("#inn, #name, #purpose").on("cleared", function (e) {
        filter(0, $(this));
    });
    $("#status-dropdown-menu li a").click(function (e) {
        filter(0, $(this));
    });
    $("#agents").parent(".dropdown").on("hide.bs.dropdown", function () {
        filter(0, $(this)); 
    });

    $("#new-task-card-inn").on("change input", function (e) {
        var inn = $(this).val();
        if (inn.length === innLengthIndividual || inn.length === innLengthLegal) {
            searchNameByInn(inn);
        } else {
            if (inn.length > innLengthLegal) {
                $(this).val(inn.substring(0, innLengthLegal)).change();
                return;
            }
            $(".new-task-card-name").text("");
        }
        validateNewTask();
    });

    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() == $(document).height()) {
            var displayedCount = $(".task-list .task-item").length;
            if (displayedCount >= pageSize) {
                filter(displayedCount);
            }
        }
    });
});

function bindContextMenu() {
    $(".task-list .task-item").contextMenu({
        menuSelector: ".task-context-menu",
        menuSelected: function (invokedOn, selectedMenuItem) {
            var taskId = $("input.id", invokedOn).val();
            if (selectedMenuItem.is(".task-context-menu-item-archivate")) {
                moveTaskToArchivePrompt(taskId);
            } else if (selectedMenuItem.is(".task-context-menu-item-repeat")) {
                repeatTaskPrompt(taskId);
            } else if (selectedMenuItem.is(".task-context-menu-item-remove")) {
                removeTaskPrompt(taskId);
            }
        },
        onBeforeMenuShow: function (invokedOn) {
            var archivateMenuItem = $(".task-context-menu .task-context-menu-item-archivate").parent();
            var repeatMenuItem = $(".task-context-menu .task-context-menu-item-repeat").parent();
            if (isArchive) {
                archivateMenuItem.hide();
                repeatMenuItem.show();
            } else {
                repeatMenuItem.hide();
                if ($("div.marker", invokedOn).length > 0) {
                    archivateMenuItem.show();
                } else {
                    archivateMenuItem.hide();
                }
            }
        }
    });
}

function searchNameByInn(inn) {
    if (lastSearchedInn === inn) {
        onNameFoundByInn(inn, lastFoundNameByInn);
        return;
    }

    postAjax(
        searchNameByInnUrl,
        JSON.stringify({ inn: inn }),
        function (resultData) {
            onNameFoundByInn(inn, resultData);
        }
    );
}

function onNameFoundByInn(inn, name) {
    $(".new-task-card-name").text(name == "" ? "ЮЛ не найдено в базе Т1000" : name);
    if (name == "") {
        $(".new-task-card-name").addClass("empty");
    } else {
        $(".new-task-card-name").removeClass("empty");
    }
    lastSearchedInn = inn;
    lastFoundNameByInn = name;
}

function filter(offset, initiator) {
    closeTaskCard();

    if (initiator != null) {
        var initiatorId = initiator.attr("id");
        if (initiatorId == "name" || initiatorId == "purpose") {
            $("#inn").val("").change();
            $("#" + (initiatorId == "name" ? "purpose" : "name") + "").val("").change();

            $("#agents input[type='checkbox']").each(function () {
                $(this).prop("checked", false).change();
            });

            $("#status-dropdown-menu li.selected").removeClass("selected");
            $("#status-dropdown-menu li:first").addClass("selected");
        } else {
            $("#name").val("").change();
            $("#purpose").val("").change();
        }
    }

    var selectedStatus = $("#status-dropdown-menu li.selected > a").data("value");
    var offsetValue = offset ? offset : 0;
    var data = JSON.stringify({
        taskFilter: {
            Offset: offsetValue,
            Limit: pageSize,            
            Type: selectedStatus,
            Inn: $("#inn").val(),
            Name: $("#name").val(),
            Purpose: $("#purpose").val(),
            AgentIds: getCheckedIds($("#agents input[type='checkbox']")),
            IsArchive: isArchive,
        },
        date: null
    });

    postAjax(
        getTasksUrl,
        data,
        function (resultData) {
            var taskList = $(".task-list");

            if (offsetValue > 0) {
                taskList.html(taskList.html() + resultData);
            } else {
                taskList.html(resultData);
            }

            bindContextMenu();
        }
    );
}

function validateNewTask() {
    var inn = $("#new-task-card-inn").val();
    var agentIds = getCheckedIds($("#new-task-card-agents input[type='checkbox']"));

    if (inn == null || inn.length !== 10 && inn.length !== 12 || agentIds.length === 0) {
        $(".btn-create-new-task").attr("disabled", "disabled");
        return null;
    } else {
        $(".btn-create-new-task").removeAttr("disabled");
        var nameElement = $(".new-task-card-name");
        var name = nameElement.hasClass("empty") ? "" : nameElement.text();
        return {
            task: {
                Inn: inn,
                Name: name,
                PurposeOfPayment: $("#new-task-card-purpose").val(),
                AgentIds: agentIds
            }
        }
    }
}

function createNewTask() {
    var rawData = validateNewTask();
    if (rawData == null)
        return;

    $(".btn-create-new-task").attr("disabled", "disabled");

    var data = JSON.stringify(rawData);

    postAjax(
        createTaskUrl,
        data,
        function (resultData) {
            var taskList = $(".task-list");
            taskList.html(resultData + taskList.html());
            closeNewTaskDialog();
            checkIfAnyTasksExist();
        },
        function () {
            $(".btn-create-new-task").removeAttr("disabled");
        }
    );
}

function moveTaskToArchivePrompt(taskId) {
    showDialog(
        null,
        false,
        "Перенести задание в архив?",
        "Перенести",
        "Отменить",
        function () {
            moveTaskToArchive(taskId);
        },
        null
    )
}
function moveTaskToArchive(taskId) {
    postAjax(
        moveTaskToArchiveUrl,
        JSON.stringify({ taskId: taskId }),
        function () {
            $(".task-item input.id[value='" + taskId + "']").closest(".task-item").remove();
            checkIfAnyTasksExist();
        }
    );
}

function repeatTaskPrompt(taskId) {
    showDialog(
        null,
        false,
        "Отправить ЮЛ на повторную проверку?",
        "Отправить",
        "Отменить",
        function () {
            repeatTask(taskId);
        },
        null
    )
}
function repeatTask(taskId) {
    postAjax(
        repeatTaskUrl,
        JSON.stringify({ taskId: taskId })
    );
}

function removeTaskPrompt(taskId) {
    showDialog(
        null,
        false,
        "Удалить задание?",
        "Удалить",
        "Отменить",
        function () {
            removeTask(taskId);
        },
        null
    )
}
function removeTask(taskId) {
    postAjax(
        removeTaskUrl,
        JSON.stringify({ taskId: taskId }),
        function (resultData) {
            $(".task-item input.id[value='" + taskId + "']").closest(".task-item").remove();
            checkIfAnyTasksExist();
        }
    );
}

function openTaskCard(taskElement) {
    $(".task-item.task-selected").removeClass("task-selected");
    taskElement.addClass("task-selected");

    $(".task-card .inn").text($(".inn", taskElement).text());
    $(".task-card .date").text($(".date", taskElement).text());

    var purpose = $(".purpose", taskElement).val();

    if (purpose != null && purpose != "") {
        $(".purpose-of-payment-wrap").show();
        $(".purpose-of-payment").text(purpose);
    } else {
        $(".purpose-of-payment-wrap").hide();
        $(".purpose-of-payment").text("");
    }

    postAjax(
        getResolutionsUrl,
        JSON.stringify({ taskId: $(".id", taskElement).val() }),
        function (resultData) {
            var resolutions = $(".task-card .resolutions");

            resolutions.html(resultData)

            $(".task-card").show();
        }
    );
}
function closeTaskCard() {
    $(".task-card").hide();
    $(".task-item.task-selected").removeClass("task-selected");
}

function openNewTaskDialog() {
    $(".overlay").show();

    $("#new-task-card-inn").val("").change();
    $("#new-task-card-purpose").val("").change();
    $("#new-task-card-agents input[type='checkbox']").each(function () {
        var checkbox = $(this);
        if (checkbox.prop("checked") === true) {
            checkbox.prop("checked", false).change();
        }
    });
    $(".new-task-card-name").text("");

    $(".new-task-dialog").show();
    $("#new-task-card-inn").focus();
}
function closeNewTaskDialog() {
    $(".new-task-dialog").hide();
    $(".overlay").hide();
}

function getCheckedIds(checkboxes) {
    var ids = [];
    checkboxes.each(function () {
        var checkbox = $(this);
        if (checkbox.prop("checked") === true) {
            ids.push(checkbox.attr("id"));
        }
    });
    return ids;
}

function enterKeyFilterHandler(e) {
    var keycode = (e.keyCode ? e.keyCode : e.which);
    if (keycode == '13') {
        filter(0, $(this));
    }
}

function downloadImage(img) {
    loading(true);

    var data = img.attr("src");
    var name = img.data("name");
    var mimeType = img.data("mime-type");
    download(data, name, mimeType);

    loading(false);
}

function checkIfAnyTasksExist() {
    if ($(".task-item").length > 0) {
        $(".no-tasks").addClass("hidden");
    } else {
        $(".no-tasks").removeClass("hidden");
    }
}
