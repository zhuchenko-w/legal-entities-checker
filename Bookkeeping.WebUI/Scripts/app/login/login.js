$(function () {
    $(".btn-login").click(function () {
        login();
    });

    $(".login-form-input").on("change input", function (e) {
        validate();
    }).keypress(enterKeyFilterHandler)
        .focus(function () {
            $("span[for='" + $(this).attr("id") + "']").addClass("focused");
        }).blur(function () {
            $("span[for='" + $(this).attr("id") + "']").removeClass("focused");
        });

    setTimeout(function () {
        $("#username").focus();
    }, 500);
});

function validate(getResult) {
    var values = new Object();
    $(".login-form-input").each(function () {
        var val = $(this).val();
        if (val == "") {
            $(".btn-login").addClass("login-disabled");
            return false;
        } else {
            values[$(this).attr("id")] = val;
        }
    });

    if (Object.keys(values).length == 2) {
        $(".btn-login").removeClass("login-disabled");
        if (getResult) {
            return {
                model: {
                    Username: values["username"],
                    Password: values["password"],
                    ReturnUrl: returnUrl
                }
            }
        }
    }

    return null;
}

function login() {
    var data = validate(true);
    if (data == null) {
        return;
    }

    var loginBtn = $(".btn-login");
    loginBtn.addClass("login-disabled");

    postAjax(
        loginUrl,
        JSON.stringify(data),
        function (resultData) {
            window.location = resultData;
        },
        function () {
            loginBtn.removeClass("login-disabled");
        }
    );
}

function enterKeyFilterHandler(e) {
    var keycode = (e.keyCode ? e.keyCode : e.which);
    if (keycode == '13') {
        e.preventDefault();
        if ($(".common-dialog").css("display") == "none") {
            login();
        } else {
            $(".common-dialog-content-title .close-common-dialog-icon");
        }
    }
}