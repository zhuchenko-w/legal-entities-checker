$(function () {
    $(".back-btn").on("click", function () {
        window.location = $(this).data("location");
    })

    $("#image").change(function () {
        showImagePreview(this);
    });

    $("body").on("click", ".resolution-image", openFileDialog);

    $(".preview img.image").click(openFileDialog);

    $(".remove-image-icon").click(removeImage);

    $("#PurposeOfPaymentComment").on("change input", function (e) {
        validate();
    });

    $(".resolution-btn-ok-wrap").click(function () {
        if ($("#Decision").val() === "true") {
            $("#Decision").val(null);
            $(".resolution-header").removeClass("green");
            $(".resolution-deny-btn").removeClass("gray");
        } else {
            $("#Decision").val(true);
            $(".resolution-ok-btn").removeClass("gray");
            $(".resolution-deny-btn").addClass("gray");
            $(".resolution-header").removeClass("red");
            $(".resolution-header").addClass("green");
        }

        validate();
    });

    $(".resolution-btn-deny-wrap").click(function () {
        if ($("#Decision").val() === "false") {
            $("#Decision").val(null);
            $(".resolution-header").removeClass("red");
            $(".resolution-ok-btn").removeClass("gray");
        } else {
            $("#Decision").val(false);
            $(".resolution-deny-btn").removeClass("gray");
            $(".resolution-ok-btn").addClass("gray");
            $(".resolution-header").removeClass("green");
            $(".resolution-header").addClass("red");
        }

        validate();
    });

    $(".save-btn").click(function () {
        if ($("#Decision").val() == "" || $("#PurposeOfPaymentComment").val() == "" && $("#PurposeOfPayment").val() != "") {
            return;
        }

        $(".save-btn").addClass("gray");
        $(".save-btn").attr("disabled", "disabled");

        saveResolution();
    });
});

function validate() {
    var canSave =
        $("#Decision").val() != ""
        && ($("#PurposeOfPayment").val() == "" || $("#PurposeOfPaymentComment").val() != "" && $("#PurposeOfPayment").val() != "");

    if (canSave) {
        $(".save-btn").removeClass("gray");
        $(".save-btn").removeAttr("disabled");
    } else {
        $(".save-btn").addClass("gray");
        $(".save-btn").attr("disabled", "disabled");
    }
}

function openFileDialog() {
    $("#image").trigger("click");
}

function saveResolution() {
    var files = $("#image").get(0).files;
    var purpose = $("#PurposeOfPaymentComment");
    var data = new FormData();
    data.append("ImageFile", files[0]);
    data.append("Id", $("#Id").val());
    data.append("ImageData", $("#ImageData").val());
    data.append("MimeType", $("#MimeType").val());
    data.append("ImageName", $("#ImageName").val());
    data.append("Note", $("#Note").val());
    data.append("Decision", $("#Decision").val());
    data.append("PurposeOfPayment", $("#PurposeOfPayment").val());
    data.append("PurposeOfPaymentComment", purpose.length > 0 ? purpose.val() : "");

    postAjax(
        saveResolutionUrl,
        data,
        function (resultData) {
            window.location = resultData;
        },
        function () {
            $(".save-btn").removeClass("gray");
            $(".save-btn").removeAttr("disabled");
        },
        false,
        false
    );
}

function showImagePreview(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(".resolution-image-upload-icon").hide();
            $(".preview").show()
            $(".preview img.image").attr("src", e.target.result);
            $("body").off("click", ".resolution-image");
            $(".resolution-image").css("cursor", "default");
        }
        reader.readAsDataURL(input.files[0]);
    } else {
        removeImage();
    }
}

function removeImage(e) {
    var image = $("#image");
    image.wrap('<form>').closest('form').get(0).reset();
    image.unwrap();

    if (e != null) {
        e.stopPropagation();
        e.preventDefault();
    }

    $("#ImageData").val("");
    $("#MimeType").val("");
    $("#ImageName").val("");

    $(".preview").hide()
    $(".resolution-image-upload-icon").show();
    $(".preview img.image").attr("src", null);
    $("body").on("click", ".resolution-image", openFileDialog);
    $(".resolution-image").css("cursor", "pointer");
}