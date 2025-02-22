import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-topnav',
  standalone: false,
  templateUrl: './topnav.component.html',
  styleUrls: ['./topnav.component.css']
})

export class TopnavComponent {
  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Read query params on component init
    this.route.queryParamMap.subscribe(params => {
      const qValue = params.get('q'); // e.g. "Action:someText"

      if (qValue) {
        const [category, term] = qValue.split(':'); // split by colon
        if (category && term) {
          if (!this.searchCategoryOptions.find(x => x == category)) {
            this.router.navigate([], {
              queryParams: { q: `${this.searchCategoryOptions[0]}:${term}` },
            });
          }
          this.searchCategory = category;
          this.searchTerm = term;
        }
      }
    });
  }

  searchCategoryOptions: string[] = [
    "all",
    "users",
    "groups",
    "organizations",
    "applications"
  ]

  searchCategory: string = this.searchCategoryOptions[0];

  searchTerm = '';

  profileImage: string | null = null;
  username: string | null = "christianhaliga@gmail.com";


  onCategorySelect(category: string): void {
    this.searchCategory = category;
  }

  onSearchEnter(searchQuery: string): void {
    if (searchQuery) {
      this.router.navigate([], {
        queryParams: { q: `${this.searchCategory}:${searchQuery}` },
      });
    } else {
      // If they cleared the box, optionally remove q
      this.router.navigate([], { queryParams: {} });
    }
  }

  onSignOut() {
    alert("TODO")
  }
}
