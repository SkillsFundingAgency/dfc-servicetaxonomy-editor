//todo: prefix buttons with gds-

(function ($) {
    'use strict';

    // Plugin default options
    var defaultOptions = {
        paragraphs: [
            {name: 'body', class: 'govuk-body'},
            {name: 'lead', class: 'govuk-body-l'},
            {name: 'small', class: "govuk-body-s"}
        ],
        headings: [
            {name: 'h1', class: 'govuk-heading-xl'},
            {name: 'h2', class: 'govuk-heading-l'},
            {name: 'h3', class: "govuk-heading-m"},
            {name: 'h4', class: "govuk-heading-s"}
        ]
    };

    // If the plugin is a button
    // function buildButtonDef (trumbowyg) {
    //     return {
    //         fn: function () {
    //             // Plugin button logic
    //         }
    //     }
    // }

    $.extend(true, $.trumbowyg, {
        // Add some translations
        langs: {
            en: {
                gds: 'GDS',
                paragraphs: {
                    'body': 'Body',
                    'lead': 'Lead',
                    'small': "Small"
                },
                paragraph: 'Paragraph',
                headings: {
                    'h1': 'h1',
                    'h2': 'h2',
                    'h3': "h3",
                    'h4': "h4",
                },
                heading: 'heading'

            }
        },
        // Register plugin in Trumbowyg
        plugins: {
            gds: {
                // Code called by Trumbowyg core to register the plugin
                init: function (trumbowyg) {
                    // Fill current Trumbowyg instance with the plugin default options
                    trumbowyg.o.plugins.gds = $.extend(true, {},
                        defaultOptions,
                        trumbowyg.o.plugins.gds || {}
                    );

                    // If the plugin is a button
                    //trumbowyg.addBtnDef('myplugin', buildButtonDef(trumbowyg));

                    trumbowyg.addBtnDef('paragraphs', {
                        dropdown: buildParagraphsDropdown(trumbowyg),
                        ico: 'p',
                        title: trumbowyg.lang.paragraph
                    });

                    trumbowyg.addBtnDef('headings', {
                        dropdown: buildHeadingsDropdown(trumbowyg),
                        //todo: new icon, just H instead of H1
                        ico: 'h1',
                        title: trumbowyg.lang.heading
                    });

                },
                // Return a list of button names which are active on current element
                tagHandler: function (element, trumbowyg) {
                    return [];
                },
                destroy: function (trumbowyg) {
                }
            }
        }
    });

    function setParagraph(trumbowyg, paragraph) {

        trumbowyg.execCmd('formatBlock', 'p')

        var selection = trumbowyg.doc.getSelection();
        $(selection.focusNode.parentNode).addClass(paragraph.class);

        // var selectionParentElement = getSelectionParentElement();
        // $(selectionParentElement).addClass(paragraph.class);

        // trumbowyg.saveRange();
        // var text = trumbowyg.getRangeText();
        // if (text.replace(/\s/g, '') !== '') {
        //     try {
        //         var parent = getSelectionParentElement();
        //         $(parent).addClass(paragraph.class);
        //     } catch (e) {
        //     }
        // }
    }

    function setHeading(trumbowyg, heading) {

        trumbowyg.execCmd('formatBlock', heading.name)

        var selection = trumbowyg.doc.getSelection();
        $(selection.focusNode.parentNode).addClass(heading.name);
    }

    //todo: use this instead?
    // Get the selection's parent
    // function getSelectionParentElement() {
    //     var parentEl = null,
    //         selection;
    //     if (window.getSelection) {
    //         selection = window.getSelection();
    //         if (selection.rangeCount) {
    //             parentEl = selection.getRangeAt(0).commonAncestorContainer;
    //             if (parentEl.nodeType !== 1) {
    //                 parentEl = parentEl.parentNode;
    //             }
    //         }
    //     } else if ((selection = document.selection) && selection.type !== 'Control') {
    //         parentEl = selection.createRange().parentElement();
    //     }
    //     return parentEl;
    // }

    function buildParagraphsDropdown(trumbowyg) {
        var dropdown = [];

        $.each(trumbowyg.o.plugins.gds.paragraphs, function (index, paragraph) {
            trumbowyg.addBtnDef('paragraph_' + paragraph.name, {
                text: '<span>' + (trumbowyg.lang.paragraphs[paragraph.name] || paragraph.name) + '</span>',
                ico: 'p',
                tag: 'p',
                fn: function () {
                    setParagraph(trumbowyg, paragraph);
                }
            });
            dropdown.push('paragraph_' + paragraph.name);
        });

        return dropdown;
    }

    function buildHeadingsDropdown(trumbowyg) {
        var dropdown = [];

        $.each(trumbowyg.o.plugins.gds.headings, function (index, heading) {
            // trumbowyg itself adds buttons names h1, h2 etc
            //todo: have global gds prefix for names
            var buttonName = 'heading_' + heading.name;
            trumbowyg.addBtnDef(buttonName, {
                text: '<span>' + (trumbowyg.lang.headings[heading.name] || heading.name) + '</span>',
                ico: heading.name,
                tag: heading.name,
                fn: function () {
                    setHeading(trumbowyg, heading);
                }
            });
            dropdown.push(buttonName);
        });

        return dropdown;
    }

})(jQuery);
