﻿$(function () {
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

$('document').ready(function () {
    $('input[id="JobProfileNew_Salarystarterperyear_Value"]').closest("form").attr('novalidate', 'novalidate');
    $('label[for="JobProfileNew_Work_Html"]').css("font-size", ".8rem");
    $('label[for="JobProfileNew_Volunteering_Html"]').css("font-size", ".8rem");
    $('label[for="JobProfileNew_Directapplication_Html"]').css("font-size", ".8rem");
    $('label[for="JobProfileNew_Work_Html"]').before("<div><label> Further routes </label></div>");

    // This is add for Sector landing page content type add time check required for html Description field.
    $('#SectorLandingPage_Description_Html').attr('required', 'true');
});
