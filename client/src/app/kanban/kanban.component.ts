import { Component, ViewChild, ViewEncapsulation } from '@angular/core';
import { extend } from '@syncfusion/ej2-base';
import { KanbanComponent, CardSettingsModel, KanbanModule } from '@syncfusion/ej2-angular-kanban';
import { kanbanData } from './data';

@Component({
    selector: 'app-kanban',
    templateUrl: 'kanban.component.html',
    styleUrl: 'kanban.component.css',
    encapsulation: ViewEncapsulation.None,
    standalone: true,
    imports: [KanbanModule]
})
export class KanbanViewComponent {
    @ViewChild('kanbanObj') kanbanObj!: KanbanComponent;
    public kanbanData: Object[] = extend([], kanbanData, undefined, true) as Object[];
    public cardSettings: CardSettingsModel = {
        contentField: 'Summary',
        headerField: 'Id',
        tagsField: 'Tags',
        grabberField: 'Color',
        footerCssField: 'ClassName'
    };
}
