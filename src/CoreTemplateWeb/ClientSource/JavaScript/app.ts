///<reference path="../../node_modules/@types/jquery/index.d.ts" />

$("[data-delete]").on("click", (event) => {
    event.preventDefault();
    let $target = $(event.target);
    if ($target.data("confirm") && !confirm($target.data("confirm"))) {
        return;
    }
    $.ajax({
        url: $target.attr("href"),
        method: "DELETE",
        success: () => {
            window.location.href = $target.data("return-url");
        }
    });
});