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
