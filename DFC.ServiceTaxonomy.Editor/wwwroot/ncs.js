$(function () {
    var ul = $("#new ul");
    var li = ul.children("li");

    li.detach().sort(function (a, b) {
        if (a.innerText > b.innerText)
            return 1;

        if (a.innerText < b.innerText)
            return -1;

        return 0;
    });

    ul.append(li);
});
