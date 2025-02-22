import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css'],
    standalone: false
})
export class AppComponent implements OnInit {
  isCollapsed = false;

  ngOnInit() { }

  sideNavCollapseToggled(toggle: boolean) {
    this.isCollapsed = toggle;
  }
}
