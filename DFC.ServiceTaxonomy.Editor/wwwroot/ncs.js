$(function () {
    var ul = $("ul#adminMenu li#new ul");
    var li = ul.children("li");

    li.detach().sort(function (a, b) {
        return (a.innerText > b.innerText) - (a.innerText < b.innerText);
    });

    ul.append(li);
});
