import { AuthService } from './../_services/auth.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  username: string;

  constructor(private authService: AuthService) {}

  ngOnInit() {}

  WelcomeUser() {
    const token = localStorage.getItem('token');
    const jwtData = token.split('.')[1];
    const decodedJwtJsonData = window.atob(jwtData);
    const decodedJwtData = JSON.parse(decodedJwtJsonData);
    this.username = decodedJwtData.unique_name;
  }

  login() {
    console.log(this.model);
    this.authService.login(this.model).subscribe(
      next => {
        console.log('Logged in successfully');
        this.WelcomeUser();
      },
      error => {
        console.log(error);
      }
    );
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !!token;
  }

  logout() {
    localStorage.removeItem('token');
    console.log('logged out');
  }

  toggle() {

  }
}
