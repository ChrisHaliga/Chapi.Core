import { Component, EventEmitter, Output } from '@angular/core';


@Component({
  selector: 'app-sidenav',
  standalone: false,
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.css']
})

export class SidenavComponent {
  isCollapsed = false;
  @Output() sidebarExpandedEvent = new EventEmitter<boolean>();

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
    this.sidebarExpandedEvent.emit(this.isCollapsed);
  }
}
