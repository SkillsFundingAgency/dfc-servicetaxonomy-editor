//todo: prefix buttons with gds-

(function ($) {
    'use strict';

    // Plugin default options
    var defaultOptions = {
        paragraphs: [
            {name: 'body', class: 'govuk-body'},
            {name: 'lead', class: 'govuk-body-l'},
            {name: 'small', class: "govuk-body-s"}
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
                paragraph: 'Paragraph'
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

                    // If the plugin is a paste handler, register it
                    // trumbowyg.pasteHandlers.push(function(pasteEvent) {
                    //     // My plugin paste logic
                    // });

                    // If the plugin is a button
                    //trumbowyg.addBtnDef('myplugin', buildButtonDef(trumbowyg));
                    trumbowyg.addBtnDef('paragraphs', {
                        dropdown: buildDropdown(trumbowyg),
                        ico: 'p',
                        title: trumbowyg.lang.paragraph
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
        //work via range?
    }

    //todo: use this instead?
    // Get the selection's parent
    function getSelectionParentElement() {
        var parentEl = null,
            selection;
        if (window.getSelection) {
            selection = window.getSelection();
            if (selection.rangeCount) {
                parentEl = selection.getRangeAt(0).commonAncestorContainer;
                if (parentEl.nodeType !== 1) {
                    parentEl = parentEl.parentNode;
                }
            }
        } else if ((selection = document.selection) && selection.type !== 'Control') {
            parentEl = selection.createRange().parentElement();
        }
        return parentEl;
    }

    function buildDropdown(trumbowyg) {
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

})(jQuery);
