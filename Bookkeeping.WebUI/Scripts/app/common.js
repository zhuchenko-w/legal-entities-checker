var DefaultRedirectTimeoutMs = 1000;

Number.prototype.padLeft = function (base, chr) {
    var len = (String(base || 10).length - String(this).length) + 1;
    return len > 0 ? new Array(len).join(chr || "0") + this : this;
}

var FormatDate = function (date, withTime) {
    return [date.getDate().padLeft(), (date.getMonth() + 1).padLeft(), date.getFullYear()].join(".") +
            (withTime ? " " + [date.getHours().padLeft(), date.getMinutes().padLeft()].join(":") : "");
}

function loading(isLoading) {
    if (isLoading) {
        $(".spinner-wrap").show();
    } else {
        $(".spinner-wrap").hide();
    }
}

function postAjax(url, data, successFunc, errorFunc, contentType, processData) {
    loading(true);
    $.ajax({
        url: url,
        type: "POST",
        data: data,
        contentType: contentType != undefined && contentType != null ? contentType : "application/json",
        processData: processData != undefined && processData != null ? processData : true,
        success: function (result) {
            if (result.error) {
                onError(result.error, result.redirectUrl);
            } else if (successFunc) {
                successFunc(result.data);
            }
        },
        error: function (xhr, error, status) {
            if (errorFunc) {
                errorFunc();
            }
            onAjaxError(xhr, error, status);
        },
        complete: function () {
            loading(false);
        }
    });
}

function onAjaxError(xhr, error, status) {
    var errorText = xhr.responseJSON != undefined ? xhr.responseJSON.Message : "";

    if (errorText == "" && error != null && error != "") {
        errorText = error != "error" || error == "error" && status != null && status != "" ? error : status;
    }

    onError(errorText != "" ? "Произошла ошибка: " + errorText : "При запросе на сервер произошла ошибка");
}

function onError(message, redirectUrl, redirectTimeoutMs) {
    showDialog(
        "Ошибка",
        true,
        message,
        null,
        null,
        null,
        null
    );
    
    if (redirectUrl != null && redirectUrl != "") {
        setTimeout(function () {
            window.location.href = redirectUrl
        }, redirectTimeoutMs == null ? DefaultRedirectTimeoutMs : redirectTimeoutMs);
    }
}

function showDialog(titleText, isClosable, bodyText, confirmText, cancelText, confirmFunc, cancelFunc) {
    var dialog = $(".common-dialog");

    var titleIsVisible = titleText != null && titleText != "";
    if (titleIsVisible || isClosable) {
        $(".common-dialog-content-title").show();

        var titleElement = $(".common-dialog-content-title .common-dialog-title");
        if (titleIsVisible) {
            titleElement.show();
            titleElement.text(titleText);
        } else {
            titleElement.hide();
        }

        closeIconElement = $(".common-dialog-content-title .close-common-dialog-icon");
        if (isClosable) {
            closeIconElement.show();
        } else {
            closeIconElement.hide();
        }
    } else {
        $(".common-dialog-content-title").hide();
    }

    $(".common-dialog-content-body .common-dialog-content-body-text").text(bodyText);

    $(".common-dialog-content-footer", dialog).off("click");
    var confirmBtnIsVisible = confirmFunc && confirmText != null && confirmText != "";
    var cancelBtnIsVisible = cancelText != null && cancelText != "";
    if (confirmBtnIsVisible || cancelBtnIsVisible) {
        $(".common-dialog-content-footer").show();

        var confirmBtn = $(".common-dialog-content-footer .btn-confirm");
        if (confirmBtnIsVisible) {
            confirmBtn.show();
            confirmBtn.text(confirmText);
            $(".common-dialog-content-footer", dialog).on("click", ".btn-confirm", function () {
                dialog.hide();
                confirmFunc();
            });
        } else {
            confirmBtn.hide();
        }

        var cancelBtn = $(".common-dialog-content-footer .btn-cancel");
        if (cancelBtnIsVisible) {
            cancelBtn.show();
            cancelBtn.text(cancelText);
            $(".common-dialog-content-footer", dialog).on("click", ".btn-cancel", cancelFunc ? cancelFunc : function () { dialog.hide(); });
        } else {
            cancelBtn.hide();
        }
    } else {
        $(".common-dialog-content-footer").hide();
    }

    dialog.show();
}

function openNav() {
    $(".overlay").show();
    $("#mainSidenav").show();
}
function closeNav() {
    if ($("#mainSidenav").css('display') != 'none') {
        $("#mainSidenav").hide();
        $(".overlay").hide();
    }
}

function getPosition(elementSelector, mousePosition, direction, scrollDirection) {
    var windowSize = $(window)[direction]();
    var scrollPosition = $(window)[scrollDirection]();
    var elementSize = $(elementSelector)[direction]();
    var position = mousePosition + scrollPosition;

    if (mousePosition + elementSize > windowSize && elementSize < mousePosition) {
        position -= elementSize;
    }

    return position;
}

function getPostPasteText (element, pastedText) {
    var selectionStart = element[0].selectionStart;
    var selectionEnd = element[0].selectionEnd;

    var oldText = element.val();
    var leftPart = oldText.substr(0, selectionStart);
    var rightPart = oldText.substr(selectionEnd, oldText.length);

    return leftPart + pastedText + rightPart;
};

$(function () {
    $(document).on("keypress", "form", function (e) {
        return e.keyCode != 13;
    });

    $(".log-out-icon").click(function () {
        window.location = "/Account/Logout";
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

    $("body").on("click", ".label-check label", function (e) {
        var checkbox = $("input[type='checkbox']", $(this).closest(".checkbox"));
        var value = checkbox.prop("checked");
        checkbox.prop("checked", !value).change();
        return false;
    });

    $(".dropdown-checkbox-menu input:checkbox").change(function () {
        var container = $(this).closest(".labeled-dropdown-group");
        var dropdown = $(this).closest(".dropdown");
        var button = $("button", dropdown);
        var checkboxes = $(".checkbox", dropdown);

        var codes = [];
        checkboxes.each(function () {
            var checkbox = $(this);
            if ($("input:checkbox", checkbox).prop("checked") === true) {
                codes.push($("label", checkbox).text());
            }
        });

        var buttonText = "";
        if (codes.length === 0) {
            buttonText = button.data("none-text");
        } else if (codes.length === checkboxes.length) {
            buttonText = button.data("all-text");
        } else {
            buttonText = codes.join(", ");
        }
        button.html(buttonText + $(".caret", button)[0].outerHTML);

        if (!container.hasClass("always-set")) {
            if (codes.length === 0) {
                container.addClass("unset");
            } else {
                container.removeClass("unset");
            }
        }
    });

    $(".labeled-dropdown").on("show.bs.dropdown", function () {
        var container = $(this).closest(".labeled-dropdown-group");
        var label = $(".labeled-dropdown-label", container);
        if (!container.hasClass("always-set")) {
            label.css("visibility", "visible");
        }
        label.addClass("focused");
    }).on("hide.bs.dropdown", function () {
        var container = $(this).closest(".labeled-dropdown-group");
        var label = $(".labeled-dropdown-label", container);
        label.css("visibility", "");
        label.removeClass("focused");
    });

    $(".labeled-dropdown-toggle").focus(function () {
        var container = $(this).closest(".labeled-dropdown-group");
        var label = $(".labeled-dropdown-label", container);
        if (!container.hasClass("always-set")) {
            label.css("visibility", "visible");
        }
        label.addClass("focused");
    }).blur(function () {
        var container = $(this).closest(".labeled-dropdown-group");
        if (!$(".dropdown", container).hasClass("open")) {
            var label = $(".labeled-dropdown-label", container);
            label.css("visibility", "");
            label.removeClass("focused");
        }
    });
    $(".labeled-dropdown-menu li a").click(function () {
        var dropdown = $(this).closest(".dropdown");
        $("li", dropdown).removeClass("selected");
        $(this).parent().addClass("selected");
        $(".btn:first-child", dropdown).html($(this).text() + ' <span class="caret"></span>');
    });

    $('textarea').each(function () {
        this.setAttribute('style', 'height:auto; overflow-y:hidden;');
        //this.setAttribute('style', 'height:' + (this.scrollHeight) + 'px;overflow-y:hidden;');
    }).on('input', function () {
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    });

    $(document).one("focus.auto-expand", "textarea.auto-expand", function () {
        var savedValue = this.value;
        this.value = "";
        this.baseScrollHeight = this.scrollHeight;
        this.value = savedValue;
    }).on("input.auto-expand", "textarea.auto-expand", function () {
        var minRows = this.getAttribute("data-min-rows") | 0;
        this.rows = minRows;
        rows = Math.ceil((this.scrollHeight - this.baseScrollHeight) / (parseInt($(this).css("font-size")) + 2));
        this.rows = minRows + rows;
    });

    $("form :input").focus(function () {
        var label = $("label[for='" + this.id + "']", $(this).closest(".material-form-group"));
        label.show().addClass("labelfocus");
        $("span.short-label-text", label).css("color", "rgb(0, 131, 143)");
    }).blur(function () {
        var label = $("label[for='" + this.id + "']", $(this).closest(".material-form-group"));
        if ($(this).val().length === 0) {
            label.fadeIn().removeClass("labelfocus");
        } else {
            $("span.short-label-text", label).css("color", "#948d8d");//todo: hide css behind classes, generalize material and search inputs
        }
    });

    $("form :input").focus(function () {
        var label = $("label[for='" + this.id + "']", $(this).closest(".search-form-group"));
        label.show().addClass("labelfocus");
        $("span.short-label-text", label).css("color", "rgb(0, 131, 143)");
    }).blur(function () {
        var label = $("label[for='" + this.id + "']", $(this).closest(".search-form-group"));
        if ($(this).val().length === 0) {
            label.fadeIn().removeClass("labelfocus");
        } else {
            $("span.short-label-text", label).css("color", "#948d8d");//todo: hide css behind classes, generalize material and search inputs
        }
    });
    $(".search-form-control, .material-form-control").on("change input", function (e) {
        var control = $(this).closest(".right-inner-addon");
        if ($(this).val().length !== 0) {
            control.addClass("filled");
        }
        else {
            control.removeClass("filled");
        }
        if (!$(this).is(":focus")) {
            $(this).trigger("blur");
        }
    });
    $(".clear-search-icon").click(function () {
        $(".search-form-control", $(this).closest(".right-inner-addon"))
            .val("")
            .change()
            .blur()
            .trigger("cleared");
    });


    $(".common-dialog-content-title .close-common-dialog-icon").click(function () {
        $(this).closest(".common-dialog").hide();
    });

    $("textarea, input[type='text'], input[type='search']").each(function () {
        if ($(this).val() != null && $(this).val() != '') {
            $("label", $(this).closest(".form-group")).addClass("labelfocus");
        }
    });

    $(".integer-input").keydown(function (e) {
        var keyCode = e.keyCode || e.charCode;
            // allow: backspace, delete, tab, escape and enter
        if ($.inArray(keyCode, [46, 8, 9, 27, 13, 110]) !== -1 ||
            // allow: Ctrl+A, Command+A
            (keyCode === 65 && (e.ctrlKey === true || e.metaKey === true)) ||
            // allow: Ctrl+C, Ctrl+V, Ctrl+Z
            (e.ctrlKey === true && (keyCode === 86 || keyCode === 67 || keyCode === 90)) ||
            // allow: home, end, left, right, down, up
            (keyCode >= 35 && keyCode <= 40)) {
            return;
        }
        // restrict all keys except numeric
        if ((e.shiftKey || (keyCode < 48 || keyCode > 57)) && (keyCode < 96 || keyCode > 105)) {
            e.preventDefault();
        }
    }).on("paste", function (e) {
        e.preventDefault();
        var pasteText = e.originalEvent.clipboardData.getData("text");
        if (new RegExp("^[0-9]+$").test(pasteText)) {
            var maxLength = parseInt($(this).attr("maxlength"));
            if (maxLength > 0) {
                var postPasteText = getPostPasteText($(this), pasteText);
                pasteText = postPasteText.substring(0, maxLength);
            }
            $(this).val(pasteText).change();
        }
    });

    var menus = {};
    $.fn.contextMenu = function (settings) {
        var $menu = $(settings.menuSelector);
        $menu.data("menuSelector", settings.menuSelector);

        if ($menu.length === 0)
            return;

        menus[settings.menuSelector] = {
            $menu: $menu,
            settings: settings
        };

        $(document).click(function (e) {
            hideAll();
        });
        $(document).on("contextmenu", function (e) {
            var $ul = $(e.target).closest("ul");
            if ($ul.length === 0 || !$ul.data("menuSelector")) {
                hideAll();
            }
        });

        (function (element, menuSelector) {
            element.on("contextmenu", function (e) {
                if (e.ctrlKey)
                    return;

                hideAll();

                var menu = getMenu(menuSelector);
                callOnBeforeMenuShow(menu, $(e.currentTarget));
                menu.$menu
                    .data("invokedOn", $(e.currentTarget))
                    .show()
                    .css({
                        position: "absolute",
                        left: getMenuPosition(e.clientX, "width", "scrollLeft"),
                        top: getMenuPosition(e.clientY, "height", "scrollTop")
                    })
                    .off("click")
                    .on("click", "a, span", function (e) {
                        menu.$menu.hide();

                        var $invokedOn = menu.$menu.data("invokedOn");
                        var $selectedMenu = $("a", $(e.target).closest("li"));

                        callOnMenuHide(menu);
                        menu.settings.menuSelected.call(this, $invokedOn, $selectedMenu);

                        return false;
                    });

                callOnMenuShow(menu);
                return false;
            });
        })($(this), settings.menuSelector);

        function getMenu(menuSelector) {
            var menu = null;
            $.each(menus, function (i_menuSelector, i_menu) {
                if (i_menuSelector == menuSelector) {
                    menu = i_menu
                    return false;
                }
            });
            return menu;
        }

        function hideAll() {
            $.each(menus, function (menuSelector, menu) {
                menu.$menu.hide();
                callOnMenuHide(menu);
            });
        }

        function callOnBeforeMenuShow(menu, invokedOn) {
            if (invokedOn && menu.settings.onBeforeMenuShow) {
                menu.settings.onBeforeMenuShow.call(this, invokedOn);
            }
        }
        function callOnMenuShow(menu) {
            var $invokedOn = menu.$menu.data("invokedOn");
            if ($invokedOn && menu.settings.onMenuShow) {
                menu.settings.onMenuShow.call(this, $invokedOn);
            }
        }
        function callOnMenuHide(menu) {
            var $invokedOn = menu.$menu.data("invokedOn");
            menu.$menu.data("invokedOn", null);
            if ($invokedOn && menu.settings.onMenuHide) {
                menu.settings.onMenuHide.call(this, $invokedOn);
            }
        }

        function getMenuPosition(mouse, direction, scrollDir) {
            return getPosition(settings.menuSelector, mouse, direction, scrollDir);
        }

        return this;
    };
});