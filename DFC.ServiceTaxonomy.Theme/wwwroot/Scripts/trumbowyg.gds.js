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
        ],
        fontWeights: [
            {name: 'bold', class: 'govuk-!-font-weight-bold'},
            {name: 'regular', class: 'govuk-!-font-weight-regular'}
        ],
        fontSizes: [
            {name: '14px', class: 'govuk-!-font-size-14'},
            {name: '16px', class: 'govuk-!-font-size-16'},
            {name: '19px', class: 'govuk-!-font-size-19'},
            {name: '24px', class: 'govuk-!-font-size-24'},
            {name: '27px', class: 'govuk-!-font-size-27'},
            {name: '36px', class: 'govuk-!-font-size-36'},
            {name: '48px', class: 'govuk-!-font-size-48'},
            {name: '80px', class: 'govuk-!-font-size-80'},
        ]
    };

    $.extend(true, $.trumbowyg, {
        // Add some translations
        langs: {
            en: {
                gds: 'GDS',
                paragraph: 'Paragraph',
                paragraphs: {
                    'body': 'Body',
                    'lead': 'Lead',
                    'small': "Small"
                },
                heading: 'Heading',
                headings: {
                    'h1': 'h1',
                    'h2': 'h2',
                    'h3': "h3",
                    'h4': "h4",
                },
                fontWeight: 'Font Weight',
                fontWeights: {
                    'bold': 'Bold',
                    'regular': 'Regular'
                },
            }
        },
        // Register plugin in Trumbowyg
        plugins: {
            gds: {
                // Code called by Trumbowyg core to register the plugin
                init: function (trumbowyg) {
                    //todo: we want to negate TheAdmin theme's css
                    //trumbowyg.o.resetCss = true;

                    // Fill current Trumbowyg instance with the plugin default options
                    trumbowyg.o.plugins.gds = $.extend(true, {},
                        defaultOptions,
                        trumbowyg.o.plugins.gds || {}
                    );

                    trumbowyg.addBtnDef('fontSize', {
                        dropdown: buildFontSizesDropdown(trumbowyg),
                        ico: 'fontsize',
                        title: 'Font Size'
                    });

                    trumbowyg.addBtnDef('list', {
                        text: 'List',
                        //todo: better icon?
                        ico: 'unordered-list',
                        fn: function () {
                            trumbowyg.execCmd('insertUnorderedList');

                            var selection = trumbowyg.doc.getSelection();
                            $(selection.focusNode).closest('ul').addClass("govuk-list");
                        }
                    });

                    trumbowyg.addBtnDef('bulletList', {
                        text: 'Bulleted List',
                        ico: 'unordered-list',
                        fn: function () {
                            trumbowyg.execCmd('insertUnorderedList');

                            var selection = trumbowyg.doc.getSelection();
                            $(selection.focusNode).closest('ul').addClass("govuk-list govuk-list--bullet");
                        }
                    });

                    trumbowyg.addBtnDef('numberList', {
                        text: 'Numbered List',
                        ico: 'ordered-list',
                        fn: function () {
                            trumbowyg.execCmd('insertOrderedList');

                            var selection = trumbowyg.doc.getSelection();
                            $(selection.focusNode).closest('ol').addClass("govuk-list govuk-list--number");
                        }
                    });

                    trumbowyg.addBtnDef('paragraph', {
                        dropdown: buildParagraphsDropdown(trumbowyg),
                        ico: 'p',
                        title: trumbowyg.lang.paragraph
                    });

                    trumbowyg.addBtnDef('heading', {
                        dropdown: buildHeadingsDropdown(trumbowyg),
                        //todo: new icon, just H instead of H1
                        ico: 'h1',
                        title: trumbowyg.lang.heading
                    });

                    trumbowyg.addBtnDef('fontWeight', {
                        dropdown: buildFontWeightsDropdown(trumbowyg),
                        ico: 'strong',
                        title: trumbowyg.lang.fontWeight
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

        trumbowyg.execCmd('formatBlock', 'p');

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

        trumbowyg.execCmd('formatBlock', heading.name);

        var selection = trumbowyg.doc.getSelection();
        $(selection.focusNode.parentNode).addClass(heading.class);
    }

    // some browsers set <b>, some, <strong>, but semantic rewriting should convert to strong anyway
    //todo: govuk-!-font-weight-bold can be set on other elements such as <p> and <span>, so we could be more clever in what we do
    function setFontWeight(trumbowyg, fontWeight) {

        trumbowyg.execCmd('formatBlock', fontWeight.name);

        //todo: if heading.name == 'regular' replace <strong> with <span> containing class

        var selection = trumbowyg.doc.getSelection();
        $(selection.focusNode.parentNode).addClass(fontWeight.name);
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

    function setFontSize(trumbowyg, fontSize) {
        trumbowyg.$ed.focus();
        trumbowyg.saveRange();

        if (trumbowyg.range.startOffset === trumbowyg.range.endOffset) {
            //no text selection, add class to parent paragraph
            var selection = trumbowyg.doc.getSelection();
            $(selection.focusNode).closest('p').removeClass().addClass(fontSize.class);

            //remove any previously added span elements
            $(selection.focusNode).closest('p').find('span[class^="govuk-!-font-size"').contents().unwrap();
        } else {
            // wrap selection in <font> element so we can target it
            trumbowyg.execCmd('fontSize', '1');

            // Find <font> elements that were added and change to <span> with chosen size
            trumbowyg.$ed.find('font[size="1"]').replaceWith(function () {
                return $('<span class="' + fontSize.class + '">' + this.innerHTML + '</span>');
            });
        }

        trumbowyg.restoreRange();
    }

    function buildFontSizesDropdown(trumbowyg) {
        var dropdown = [];

        $.each(trumbowyg.o.plugins.gds.fontSizes, function (index, fontSize) {
            trumbowyg.addBtnDef('fontSize_' + fontSize.name, {
                text: '<span class="' + fontSize.class + '">' + fontSize.name + '</span>',
                //todo: find a suitable icon
                hasIcon: false,
                fn: function () {
                    setFontSize(trumbowyg, fontSize);
                }
            });
            dropdown.push('fontSize_' + fontSize.name);
        });

        return dropdown;
    }

    function buildParagraphsDropdown(trumbowyg) {
        var dropdown = [];

        $.each(trumbowyg.o.plugins.gds.paragraphs, function (index, paragraph) {
            trumbowyg.addBtnDef('paragraph_' + paragraph.name, {
                text: '<span>' + (trumbowyg.lang.paragraphs[paragraph.name] || paragraph.name) + '</span>',
                //todo: either different icon for each, or no icons
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
                //todo: don't have text for these buttons. hideButtonTexts is global, and '' is false (of course!). narrow through css?
                // t.hideButtonTexts ? '' : (btn.text || btn.title || t.lang[btnName] || btnName),
                text: ' ',
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

    function buildFontWeightsDropdown(trumbowyg) {
        var dropdown = [];

        $.each(trumbowyg.o.plugins.gds.fontWeights, function (index, fontWeight) {
            var buttonName = 'fontWeight_' + fontWeight.name;
            trumbowyg.addBtnDef(buttonName, {
                text: '<span>' + (trumbowyg.lang.fontWeights[fontWeight.name] || fontWeight.name) + '</span>',
                //todo: add icon to fontWeight, find one suitable for regular
                ico: 'strong',
                tag: 'strong',
                fn: function () {
                    setFontWeight(trumbowyg, fontWeight);
                }
            });
            dropdown.push(buttonName);
        });

        return dropdown;
    }

})(jQuery);
