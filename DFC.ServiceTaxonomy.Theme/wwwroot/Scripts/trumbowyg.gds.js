//todo: prefix buttons with gds-

(function ($) {
    'use strict';

    // Plugin default options
    var defaultOptions = {
        paragraphs: [
            { name: 'body', class: 'govuk-body' },
            { name: 'lead', class: 'govuk-body-l' },
            { name: 'small', class: "govuk-body-s" }
        ],
        headings: [
            { name: 'h1', class: 'govuk-heading-xl' },
            { name: 'h2', class: 'govuk-heading-l' },
            { name: 'h3', class: "govuk-heading-m" },
            { name: 'h4', class: "govuk-heading-s" }
        ],
        fontSizes: [
            { name: '14px', class: 'govuk-!-font-size-14' },
            { name: '16px', class: 'govuk-!-font-size-16' },
            { name: '19px', class: 'govuk-!-font-size-19' },
            { name: '24px', class: 'govuk-!-font-size-24' },
            { name: '27px', class: 'govuk-!-font-size-27' },
            { name: '36px', class: 'govuk-!-font-size-36' },
            { name: '48px', class: 'govuk-!-font-size-48' },
            { name: '80px', class: 'govuk-!-font-size-80' },
        ],
        accordion: [
            { name: 'create', tag: 'Create accordion' },
            { name: 'addRow', tag: 'Add a new row' },
            { name: 'removeRow', tag: 'Remove a row' },
            { name: 'delete', tag: 'Delete accordion' }
        ],
        tabs: [
            { name: 'create', tag: 'Create tab section' },
            { name: 'addPanel', tag: 'Add a new panel' },
            { name: 'removePanel', tag: 'Remove a panel' },
            { name: 'delete', tag: 'Delete tab section' }
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

                    trumbowyg.addBtnDef('sectionBreak', {
                        dropdown: buildSectionBreaksDropdown(trumbowyg),
                        title: 'Section Break',
                        ico: 'horizontal-rule'
                    });

                    trumbowyg.addBtnDef('links', {
                        dropdown: buildLinksDropdown(trumbowyg),
                        ico: 'link',
                        title: 'Link'
                    });

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

                    trumbowyg.addBtnDef('accordion', {
                        dropdown: buildAccordionDropdown(trumbowyg),
                        ico: 'ordered-list',
                        text: 'Accordion'
                    });

                    trumbowyg.addBtnDef('fontWeight', {
                        ico: 'strong',
                        title: trumbowyg.lang.fontWeight,
                        fn: function () {
                            setFontWeightOrSize(trumbowyg, { name: 'bold', class: 'govuk-!-font-weight-bold' });
                        }
                    });

                    trumbowyg.addBtnDef('tabs', {
                        dropdown: buildTabsDropdown(trumbowyg),
                        ico: 'ordered-list',
                        text: 'Tab'
                    });

                    setColourTitles();
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

    function setFontWeightOrSize(trumbowyg, fontWeightOrSize) {
        trumbowyg.$ed.focus();
        trumbowyg.saveRange();

        var selection = trumbowyg.doc.getSelection();
        var classSelector = fontWeightOrSize.class.substring(0, fontWeightOrSize.class.lastIndexOf('-'));

        if (trumbowyg.range.startOffset === trumbowyg.range.endOffset) {
            //no text selection, add class to parent paragraph, remove it if it's already there
            var $parent = $(selection.focusNode).closest('p');

            if ($parent.hasClass(fontWeightOrSize.class)) {
                $parent.removeClass(fontWeightOrSize.class);

                if ($parent[0].className.trim().length === 0) {
                    $parent.removeAttr('class');
                }
            } else {
                $parent.removeClass(function (index, className) {
                    return className.split(' ').filter(x => x.startsWith(classSelector)).join(' ');
                }).addClass(fontWeightOrSize.class);

                //remove any previously added span elements
                $parent.find('span[class^="' + classSelector + '"').contents().unwrap();
            }
        } else {
            //if the selection is a link, go up another level, so the link is always the inner most node
            var node = selection.focusNode.parentNode.tagName === 'A' ?
                selection.focusNode.parentNode :
                selection.focusNode;

            if (node.parentNode.tagName === 'SPAN') {
                if ($(node.parentNode).hasClass(fontWeightOrSize.class)) {
                    $(node.parentNode).removeClass(fontWeightOrSize.class);

                    if (node.parentNode.className.trim().length === 0) {
                        $(node.parentNode).contents().unwrap();
                    }
                } else {
                    $(node.parentNode).removeClass(function (index, className) {
                        return className.split(' ').filter(x => x.startsWith(classSelector)).join(' ');
                    }).addClass(fontWeightOrSize.class);
                }
            } else {
                $(node).wrap('<span class="' + fontWeightOrSize.class + '"></span>');
            }
        }

        trumbowyg.restoreRange();
    }

    function createLink(trumbowyg, openInNewTab) {
        trumbowyg.$ed.focus();
        trumbowyg.saveRange();

        if (trumbowyg.range.startOffset !== trumbowyg.range.endOffset) {
            var selection = trumbowyg.doc.getSelection();

            var options = {
                text: {
                    label: "Link Text",
                    required: true,
                    value: ''
                },
                url: {
                    label: 'URL',
                    required: true,
                    value: ''
                }
            };

            if (selection.focusNode.parentNode.tagName === 'A') {
                options.url.value = $(selection.focusNode.parentNode).attr('href');
                options.text.value = $(selection.focusNode.parentNode).text();
            }

            trumbowyg.openModalInsert(openInNewTab ? 'Link in New Tab' : 'Link', options, function (v) {
                var $link;

                if (selection.focusNode.parentNode.tagName === 'A') {
                    $link = $(selection.focusNode.parentNode);
                    $link.attr('href', v.url);
                    $link.text(v.text);
                } else {
                    $link = $('<a href="' + v.url + '" class="govuk-link"></a>');
                    $(selection.focusNode).wrap($link);
                }

                if (openInNewTab) {
                    $link
                        .attr('target', '_blank')
                        .attr('rel', 'noreferrer noopener');
                } else {
                    $link
                        .removeAttr('target')
                        .removeAttr('rel', 'noreferrer noopener');
                }

                return true;
            });
        }

        trumbowyg.restoreRange();
    }

    function createSectionBreak(trumbowyg, size, visible) {
        trumbowyg.execCmd('insertHorizontalRule');

        var classes = 'govuk-section-break';

        if (size === 'm' || size === 'l' || size === 'xl') {
            classes += ' govuk-section-break--' + size;
        }

        if (visible) {
            classes += ' govuk-section-break--visible';
        }

        var selection = trumbowyg.doc.getSelection();

        $(selection.anchorNode)
            .find('hr')
            .addClass(classes);
    }

    function accordionSelection(trumbowyg, method) {
        switch (method) {
            case 'create':
                createAccordion(trumbowyg);
                break;
            case 'addRow':
                addAccordionRow(trumbowyg);
                break;
            case 'removeRow':
                removeAccordionRow(trumbowyg);
                break;
            case 'delete':
                deleteAccordion(trumbowyg);
                break;
        }
    }

    function createAccordion(trumbowyg) {
        var selection = trumbowyg.doc.getSelection();
        $(selection.anchorNode).append(accordionBuilder());
    }

    function addAccordionRow(trumbowyg) {
        var selection = trumbowyg.doc.getSelection();
        var anchorNode = $(selection.anchorNode);
        var accordion = anchorNode.closest($("div.govuk-accordion"));
        $(accordion).append(accordionSectionBuilder(Date.now()));
    }

    function removeAccordionRow(trumbowyg) {
        var selection = trumbowyg.doc.getSelection();
        var anchorNode = $(selection.anchorNode);

        if (anchorNode.hasClass('trumbowyg-editor') || anchorNode.hasClass('govuk-accordion')) {
            return;
        }
        else {
            $(anchorNode.closest($("div.govuk-accordion__section"))).remove();
        }
    }

    function deleteAccordion(trumbowyg) {
        var selection = trumbowyg.doc.getSelection();
        var anchorNode = $(selection.anchorNode);

        $(anchorNode.closest($("div.govuk-accordion"))).remove();

    }

    function tabsSelection(trumbowyg, method) {
        switch (method) {
            case 'create':
                createTabs(trumbowyg);
                break;
            case 'addPanel':
                addPanel(trumbowyg);
                break;
            case 'removePanel':
                removePanel(trumbowyg);
                break;
            case 'delete':
                deleteTabs(trumbowyg);
                break;
        }
    }

    function createTabs(trumbowyg, method) {
        var selection = trumbowyg.doc.getSelection();
        $(selection.anchorNode).append(tabsBuilder());
    }

    function addPanel(trumbowyg) {
        var selection = trumbowyg.doc.getSelection();
        var anchorNode = $(selection.anchorNode);
        var tabs = anchorNode.closest($("div.govuk-tabs"));
        var contents = tabs.children("ul.govuk-tabs__list");
        var id = Date.now();
        $(tabs).append(panelBuilder(id));
        $(contents).append(tabContentItemBuilder(id));
    }

    function removePanel(trumbowyg) {
        var selection = trumbowyg.doc.getSelection();
        var anchorNode = $(selection.anchorNode);
        var panel = $(anchorNode.closest($("div.govuk-tabs__panel")));
        var contents = $('a[href="#' + $(panel).attr('id') + '"]').parent();
        $(panel).remove();
        $(contents).remove();
    }

    function deleteTabs(trumbowyg) {
        var selection = trumbowyg.doc.getSelection();
        var anchorNode = $(selection.anchorNode);
        $(anchorNode.closest($("div.govuk-tabs"))).remove();

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

    function buildSectionBreaksDropdown(trumbowyg) {
        trumbowyg.addBtnDef('defaultSectionBreak', {
            text: 'Section Break',
            ico: 'horizontal-rule',
            fn: function () {
                createSectionBreak(trumbowyg, 's', false);
            }
        });

        trumbowyg.addBtnDef('defaultSectionBreakVisible', {
            text: 'Section Break (Visible)',
            ico: 'horizontal-rule',
            fn: function () {
                createSectionBreak(trumbowyg, 's', true);
            }
        });

        trumbowyg.addBtnDef('mediumSectionBreak', {
            text: 'Medium Section Break',
            ico: 'horizontal-rule',
            fn: function () {
                createSectionBreak(trumbowyg, 'm', false);
            }
        });

        trumbowyg.addBtnDef('mediumSectionBreakVisible', {
            text: 'Medium Section Break (Visible)',
            ico: 'horizontal-rule',
            fn: function () {
                createSectionBreak(trumbowyg, 'm', true);
            }
        });

        trumbowyg.addBtnDef('largeSectionBreak', {
            text: 'Large Section Break',
            ico: 'horizontal-rule',
            fn: function () {
                createSectionBreak(trumbowyg, 'l', false);
            }
        });

        trumbowyg.addBtnDef('largeSectionBreakVisible', {
            text: 'Large Section Break (Visible)',
            ico: 'horizontal-rule',
            fn: function () {
                createSectionBreak(trumbowyg, 'l', true);
            }
        });

        trumbowyg.addBtnDef('xlargeSectionBreak', {
            text: 'Extra-Large Section Break',
            ico: 'horizontal-rule',
            fn: function () {
                createSectionBreak(trumbowyg, 'xl', false);
            }
        });

        trumbowyg.addBtnDef('xlargeSectionBreakVisible', {
            text: 'Extra-Large Section Break (Visible)',
            ico: 'horizontal-rule',
            fn: function () {
                createSectionBreak(trumbowyg, 'xl', true);
            }
        });

        return [
            'defaultSectionBreak',
            'defaultSectionBreakVisible',
            'mediumSectionBreak',
            'mediumSectionBreakVisible',
            'largeSectionBreak',
            'largeSectionBreakVisible',
            'xlargeSectionBreak',
            'xlargeSectionBreakVisible'
        ];
    }

    function buildLinksDropdown(trumbowyg) {
        trumbowyg.addBtnDef('createLink', {
            text: 'Link',
            ico: 'create-link',
            fn: function () {
                createLink(trumbowyg, false);
            }
        });

        trumbowyg.addBtnDef('createNewTabLink', {
            text: 'Link in New Tab',
            //todo: find a suitable icon
            ico: 'create-link',
            fn: function () {
                createLink(trumbowyg, true);
            }
        });

        return ['createLink', 'createNewTabLink', 'unlink'];
    }

    function buildFontSizesDropdown(trumbowyg) {
        var dropdown = [];

        $.each(trumbowyg.o.plugins.gds.fontSizes, function (index, fontSize) {
            trumbowyg.addBtnDef('fontSize_' + fontSize.name, {
                text: '<span class="' + fontSize.class + '">' + fontSize.name + '</span>',
                //todo: find a suitable icon
                hasIcon: false,
                fn: function () {
                    setFontWeightOrSize(trumbowyg, fontSize);
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

    function buildAccordionDropdown(trumbowyg) {
        var dropdown = [];

        $.each(trumbowyg.o.plugins.gds.accordion, function (index, heading) {
            var buttonName = 'accordion_' + heading.name;
            trumbowyg.addBtnDef(buttonName, {
                text: heading.tag,
                ico: heading.name,
                tag: heading.name,
                fn: function () {
                    accordionSelection(trumbowyg, heading.name);
                }
            });
            dropdown.push(buttonName);
        });
        return dropdown;

    }

    function buildTabsDropdown(trumbowyg) {
        var dropdown = [];

        $.each(trumbowyg.o.plugins.gds.tabs, function (index, heading) {
            var buttonName = 'tabs' + heading.name;
            trumbowyg.addBtnDef(buttonName, {
                text: heading.tag,
                ico: heading.name,
                tag: heading.name,
                fn: function () {
                    tabsSelection(trumbowyg, heading.name);
                }
            });
            dropdown.push(buttonName);
        });
        return dropdown;

    }

    function setColourTitles() {
        var colors = [
            { code: '#0b0c0c', label: 'Primary Text, Active Links, Input Border, Focus Text State' },
            { code: '#626a6e', label: 'Secondary Text' },
            { code: '#1d70b8', label: 'Links, Brand colour' },
            { code: '#003078', label: 'Hover Links' },
            { code: '#4c2c92', label: 'Visited Links' },
            { code: '#b1b4b6', label: 'Border' },
            { code: '#ffdd00', label: 'Focus State' },
            { code: '#d4351c', label: 'Error State' },
            { code: '#003a69', label: 'DfE Brand' },
            { code: '#347ca9', label: 'DfE Brand Websafe' },
            { code: '#00703c', label: 'Green' },
            { code: '#5694ca', label: 'Light Blue' },
            { code: '#f3f2f1', label: 'Light Grey' },
            { code: '#ffffff', label: 'White' },
            { code: '#6f72af', label: 'Light Purple' },
            { code: '#912b88', label: 'Bright Purple' },
            { code: '#d53880', label: 'Pink' },
            { code: '#f499be', label: 'Light Pink' },
            { code: '#f47738', label: 'Orange' },
            { code: '#b58840', label: 'Brown' },
            { code: '#85994b', label: 'Light Green' },
            { code: '#28a197', label: 'Turquoise' }
        ]

        //need to allow time for the editor to initialise, there doesn't seem to be a callback we can hook into unfortunately.
        setTimeout(function () {
            $('.trumbowyg-dropdown-foreColor button, .trumbowyg-dropdown-backColor button').each(function () {
                var color = colors.find(x => x.code === $(this).text());

                if (color) {
                    $(this).attr('title', color.label);
                }
            });
        }, 500);
    }

    function accordionBuilder() {
        var divEnd = '</div>';
        var accordionStart = '<div class=\"govuk-accordion\" data-module=\"govuk-accordion\" id=\"accordion-default\">';
        var defaultRows = [
            { id: 'Row1' },
            { id: 'Row2' }
        ];

        var html = accordionStart;

        $.each(defaultRows, function (index, row) {
            html += accordionSectionBuilder(Date.now());
        });

        html += divEnd;

        return html;
    }

    function accordionSectionBuilder(id) {
        {
            var textIdTag = '{IdValue}';
            var divEnd = '</div>';
            var accordionSection = '<div class="govuk-accordion__section">';
            var accordionHeader = '<div class="govuk-accordion__section-header">';
            var accordionH2 = '<h2 class="govuk-accordion__section-heading">';
            var accordionH2End = '</h2>';
            var accordionButton =
                '<span class="govuk-accordion__section-button" id="accordion-default-heading-' + textIdTag + '">Heading</span>';
            var accordionText = '<div id="accordion-default-content-' + textIdTag + '" class="govuk-accordion__section-content" aria-labelledby="accordion-default-heading-' + textIdTag + '"> <p class="govuk-body">This is the content for Writing well for the web.</p></div>';


            var html = accordionSection + accordionHeader + accordionH2 + accordionButton.replace(new RegExp(textIdTag, 'g'), id) + accordionH2End + divEnd;
            html += accordionText.replace(new RegExp(textIdTag, 'g'), id) + divEnd;
            return html;
        }
    }

    function tabsBuilder() {
        var tab1Id = Date.now()-1;
        var tab2Id = Date.now();
        var divEnd = '</div>';
        var tab = '<div class="govuk-tabs" data-module="govuk-tabs"><h2 class="govuk-tabs__title">Contents</h2><ul class="govuk-tabs__list">';
        tab += tabContentItemBuilder(tab1Id);
        tab += tabContentItemBuilder(tab2Id);
        tab += '</ul>';
        tab += panelBuilder(tab1Id, true);
        tab += panelBuilder(tab2Id);
        tab += '</div>';

        return tab;
    }

    function tabContentItemBuilder(id) {
        return '<li class="govuk-tabs__list-item govuk-tabs__list-item--selected"><a class="govuk-tabs__tab" href = "#' + id + '" >[Tab Title For No Javascript pages]</a></li>';
    }

    function panelBuilder(id, show) {

        var hidden = function (show) {
            return show === undefined || show === false ? ' govuk-tabs__panel--hidden' : '';
        };

        return '<div class="govuk-tabs__panel' + hidden(show) + '" id="' + id + '"><h2 class="govuk-heading-l">[Insert Tab Title Here]</h2><div>[place tab content here]</div></div>';
    }

})(jQuery);
