$(function () {
    var ul = $("#new ul");
    var li = ul.children("li");

    li.detach().sort(function (a, b) {
        if (a.innerText.toLowerCase() > b.innerText.toLowerCase())
            return 1;

        if (a.innerText.toLowerCase() < b.innerText.toLowerCase())
            return -1;

        return 0;
    });

    ul.append(li);

    //Remove anything after ## as these hints are processed
    $(".form-group .hint").each(function () {
        if ($(this).text().match('##')) {
            $(this).text($(this).text().split('##')[0]);
        }
    });

    //hide the configuration -> SEO menu item as it can't be disabled any other way I don't think??
    $('li.has-items span.title:contains("SEO")').closest('li').remove();
});

$.fn.only = function (events, callback) {
    //The handler is executed at most once for all elements for all event types.
    var $this = $(this).on(events, myCallback);
    function myCallback(e) {
        $this.off(events, myCallback);
        callback.call(this, e);
    }
    return this
};

$(document).ready(function () {
    needToConfirm = false;
    $("a").click(askConfirm);
});

$(".edit-container").only('change', function () {
    needToConfirm = true;
});

$(document).only('contentpreview:render', function () {
    needToConfirm = true;
});


function askConfirm() {
    var allowed = this.target === '_blank' || this.dataset['toggle'] === 'tab';
    if (needToConfirm && !allowed) {
        var confirm = window.confirm("Are you sure you want to navigate away from this page?\n\nYou have unsaved changes.\n\nPress OK to continue, or Cancel to stay on the current page.");
        if (!confirm) {
            return false;
        }
    }
}
