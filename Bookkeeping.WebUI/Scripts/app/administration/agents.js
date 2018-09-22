$(function () {
    $(".create-new-agent").click(function () {
        createNewAgent();
    });

    $(".agent-list").on("click", ".agent-action.edit-agent", function () {
        editAgent($(this));
    }).on("click", ".agent-action.set-agent-lock-state", function () {
        toggleAgentLockStatePrompt($(this));
    }).on("click", ".agent-action.remove-agent", function () {
        removeAgentPrompt($(this));
    }).on("click", ".agent-edit .save-edit", function () {
        saveAgent($(this));
    }).on("click", ".agent-edit .cancel-edit", function () {
        cancelAgentEdit($(this));
    }).on("change input", ".agent-text-editor", function () {
        validate($(this));
    });
});

function validate(initiator) {
    var agentItemElement = getAgentItemElement(initiator);
    var code = $(".code-edit", agentItemElement).val();
    var password = $(".password-edit", agentItemElement).val();

    if (code == "" || password.length < minPasswordLength) {
        $(".edit-button.save-edit", agentItemElement).addClass("disabled");
    } else {
        $(".edit-button.save-edit", agentItemElement).removeClass("disabled");
    }
}

function setAgentEditMode(initiator, on, agentItemElement) {
    if (agentItemElement == null) {
        agentItemElement = getAgentItemElement(initiator);
    }

    if (on) {
        agentItemElement.addClass("edit");
    } else {
        agentItemElement.removeClass("edit");
        $(".agent-text-editor").val("").change();
    }
}

function createNewAgent() {
    closeActiveAgentEditor();

    if (!$(".agent-list .agent-item:first").hasClass("edit")) {
        var newItem = $($(".new-agent-item").html()).prependTo($(".agent-list"));
        setAgentEditMode(null, true, newItem);
    }

    checkIfAnyAgentsExist();
}

function editAgent(initiator) {
    closeActiveAgentEditor();

    var agentItemElement = getAgentItemElement(initiator);
    $(".code-edit", agentItemElement).val($(".code", agentItemElement).text()).change();
    $(".password-edit", agentItemElement).val($(".password", agentItemElement).text()).change();
    setAgentEditMode(null, true, agentItemElement);
}

function closeActiveAgentEditor() {
    var activeAgentEditor = $(".agent-item.edit .agent-edit");
    if (activeAgentEditor.length > 0) {
        cancelAgentEdit(activeAgentEditor);
    }
}

function cancelAgentEdit(initiator) {
    var agentItemElement = getAgentItemElement(initiator);
    var id = $(".id", agentItemElement).val();
    if (id === "" || id === "0") {
        agentItemElement.remove();
        checkIfAnyAgentsExist();
    } else {
        setAgentEditMode(null, false, agentItemElement);
    }
}

function saveAgent(initiator) {
    var agentItemElement = getAgentItemElement(initiator);
    var code = $(".code-edit", agentItemElement).val();
    var password = $(".password-edit", agentItemElement).val();

    if (code == "" || password.length < minPasswordLength) {
        return;
    }

    var id = $(".id", agentItemElement).val();
    var isNew = id === "" || id === "0";
    var model = {
        Code: code,
        Password: password
    };
    var data = isNew
        ? JSON.stringify({
            model: model
        })
        : JSON.stringify({
            agentId: id,
            model: model
        });

    postAjax(
        isNew ? createAgentUrl : editAgentUrl,
        data,
        function (resultData) {
            if (isNew) {
                $(".id", agentItemElement).val(resultData.Id);
                $(".code", agentItemElement).text(resultData.Code);
                $(".password", agentItemElement).text(resultData.Password);
            } else {
                $(".code", agentItemElement).text(code);
                $(".password", agentItemElement).text(password);
            }
            setAgentEditMode(initiator, false);
        }
    );
}

function toggleAgentLockStatePrompt(initiator) {
    var agentItemElement = getAgentItemElement(initiator);
    var agentActionsElement = $(".agent-actions", agentItemElement);
    var isCurrentlyLocked = agentActionsElement.hasClass("locked");

    showDialog(
        null,
        false,
        isCurrentlyLocked ? "Разрешить доступ агенту?" : "Ограничить доступ агенту?",
        isCurrentlyLocked ? "Разрешить" : "Ограничить",
        "Отмена",
        function () {
            toggleAgentLockState($(".id", agentItemElement).val(), isCurrentlyLocked, agentActionsElement);
        },
        null
    )
}
function toggleAgentLockState(id, isCurrentlyLocked, agentActionsElement) {
    var dataJson = {
        agentId: id,
        isLocked: !isCurrentlyLocked
    };

    postAjax(
        setAgentLockStateUrl,
        JSON.stringify(dataJson),
        function (resultData) {
            if (dataJson.isLocked) {
                agentActionsElement.removeClass("unlocked").addClass("locked");
            } else {
                agentActionsElement.removeClass("locked").addClass("unlocked");
            }
        }
    );
}

function removeAgentPrompt(initiator) {
    showDialog(
        null,
        false,
        "Вы хотите удалить учетную запись агента и все его резолюции?",
        "Удалить",
        "Отмена",
        function () {
            removeAgent(initiator);
        },
        null
    )
}
function removeAgent(initiator) {
    var agentItemElement = getAgentItemElement(initiator);
    var id = $(".id", agentItemElement).val();

    postAjax(
        removeAgentUrl,
        JSON.stringify({ agentId: id }),
        function (redirectUrl) {
            agentItemElement.remove();
            checkIfAnyAgentsExist();
            if (redirectUrl != null) {
                window.location.href = redirectUrl;
            }
        }
    );
}

function getAgentItemElement(child) {
    return child.closest(".agent-item");
}

function checkIfAnyAgentsExist() {
    if ($(".agent-item").length > 1) {
        $(".no-agents-wrap").addClass("hidden");
    } else {
        $(".no-agents-wrap").removeClass("hidden");
    }
}